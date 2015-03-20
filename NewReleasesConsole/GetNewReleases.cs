using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;
using Async;
using DiscogsNet;
using DiscogsNet.Api;
using DiscogsNet.Model.Search;
using DiscogsNet.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NewMusicSunshine.Core;
using NewReleases.Service;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;



namespace NewReleasesConsole
{
    public class GetNewReleases
    {
        string accessKeyId = "";
        string secretKey = "";
        string associateTag = "newmussun-20";
        public static string UserAgent = @"NewMusicSunshine/0.1 +https://github.com/shiitake/NewMusicSunshine";

        private string BuildMBSearchUrl(string arid)
        {
            DateTime date = DateTime.Today;
            var startDate = date.ToString("yyyy-MM-dd");
            var endDate = date.AddMonths(6).ToString("yyyy-MM-dd");
            var dateRange = "date:[" + startDate + " TO " + endDate + "]";
            return String.Format("arid:{0} AND {1}", arid, dateRange);
        }

        private string BuildMBReleaseUrl(string arid)
        {
            var asinExists = "asin:([0 TO 9] [a TO z])";
            var primaryType = "primarytype:album";
            return String.Format("arid:{0} AND {1} AND {2}", arid, primaryType, asinExists);
        }
        
        
        public List<Artist> GetArtistListFromMusicBrainz(string name)
        {
            List<Artist> artistList = new List<Artist>();
            XDocument docResponse = null;
            var baseuri = "http://musicbrainz.org/ws/2/artist/?query=artist:";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(baseuri + name);
            req.Method = "GET";
            req.UserAgent = UserAgent;
            //req.ContentType = "application/x-www-form-urlencoded";

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (XmlReader reader = XmlReader.Create(resp.GetResponseStream()))
                {
                    docResponse = XDocument.Load(reader);
                }
            }

            if (docResponse != null)
            {
                //checked release count                
                XNamespace aw = docResponse.Root.Name.NamespaceName;
                var count = int.Parse(docResponse.Element(aw + "metadata").Element(aw + "artist-list").Attribute("count").Value);
                if (count > 0)
                {
                    IEnumerable<XElement> artists = null;
                    artists = (
                        from artist in
                            docResponse.Element(aw + "metadata").Element(aw + "artist-list").Elements(aw + "artist")
                        select artist
                        );
                    foreach (XElement artist in artists)
                    {
                        var art = new Artist();
                        art.Name = artist.Element(aw + "name").Value;
                        art.Id = artist.Attribute("id").Value;
                        //art.Country = artist.Element(aw + "area")
                        //        .Element(aw + "name").Value ?? "";
                        artistList.Add(art);
                    }
                }
            }
            return artistList;
        }

        public List<Release> GetNewReleasesFromMusicBrainz(string arid = "ad0ecd8b-805e-406e-82cb-5b00c3a3a29e")
        {
            //arid = "ad0ecd8b-805e-406e-82cb-5b00c3a3a29e";
            List<Release> releaseList = new List<Release>();
            XDocument docResponse = null;
            var url = BuildMBSearchUrl(arid);
            var baseuri = "http://musicbrainz.org/ws/2/release/";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(baseuri + "?query=" + WebUtility.UrlEncode(url));
            req.Method = "GET";
            req.UserAgent = UserAgent;
            //req.ContentType = "application/x-www-form-urlencoded";

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (XmlReader reader = XmlReader.Create(resp.GetResponseStream()))
                {
                    docResponse = XDocument.Load(reader);
                }
            }
            
            if (docResponse != null)
            {
                //checked release count
                XNamespace aw = docResponse.Root.Name.NamespaceName;
                var count = int.Parse(docResponse.Element(aw + "metadata").Element(aw +"release-list").Attribute("count").Value);
                if (count > 0)
                {
                    releaseList = ProcessReleaseData(docResponse);
                }
            }
            return releaseList;
        }

        public List<Release> GetAsinDataFromMusicBrainz(string arid)
        {
            List<Release> releaseList = new List<Release>();
            XDocument docResponse = null;
            var url = BuildMBReleaseUrl(arid);
            var baseuri = "http://musicbrainz.org/ws/2/release/";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(baseuri + "?query=" + WebUtility.UrlEncode(url));
            req.Method = "GET";
            req.UserAgent = UserAgent;
            
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (XmlReader reader = XmlReader.Create(resp.GetResponseStream()))
                {
                    docResponse = XDocument.Load(reader);
                }
            }

            if (docResponse != null)
            {
                //checked release count
                XNamespace aw = docResponse.Root.Name.NamespaceName;
                var count = int.Parse(docResponse.Element(aw + "metadata").Element(aw + "release-list").Attribute("count").Value);
                if (count > 0)
                {
                    releaseList = ProcessReleaseData(docResponse);
                }
            }
            return releaseList;
        }

        public List<Release> ProcessReleaseData(XDocument data)
        {
            List<Release> releaseList = new List<Release>();
            string asinList = "";
            XNamespace aw = data.Root.Name.NamespaceName;
            var count = int.Parse(data.ElementOrEmpty(aw, "metadata")
                .ElementOrEmpty(aw, "release-list")
                .Attribute("count").Value);
            if (count > 0)
            {
                IEnumerable<XElement> releases = null;
                releases = (
                    from release in
                        data.ElementOrEmpty(aw, "metadata")
                        .ElementOrEmpty(aw, "release-list").Elements(aw + "release")
                    select release
                    );
                int asincount = 0;
                foreach (XElement release in releases)
                {
                    var rel = new Release();
                    rel.Name = release.ElementOrEmpty(aw, "title").Value;
                    rel.StringDate = release.ElementOrEmpty(aw, "date").Value;
                    rel.Label = release.ElementOrEmpty(aw, "label-info-list").ElementOrEmpty(aw, "label-info")
                            .ElementOrEmpty(aw, "label")
                            .ElementOrEmpty(aw, "name").Value;
                    rel.ASIN = release.ElementOrEmpty(aw, "asin").Value ?? "";
                    if (rel.ASIN.Length > 0 && asincount <= 10)
                    {
                        asinList += rel.ASIN + "%2C";
                        asincount++;
                    }
                    releaseList.Add(rel);
                }
            }
            GetAmazonArtistID(asinList);

           return releaseList;
        }

        public async void GetAmazonArtistID(string asinList)
        {
            DateTime now = DateTime.UtcNow;
            string timestamp = now.ToString("yyyy-MM-ddTHH:mm:ss.000Z");
            
            // build query
            var queryString = BuildQueryString(asinList, WebUtility.UrlEncode(timestamp));
            var canonicalRequest = BuildCanonicalRequest(queryString);
           
            // sign the data
            var signature = GetRequestSignature(canonicalRequest, secretKey);

            XDocument docResponse = null;
            Uri baseuri = new Uri("http://webservices.amazon.com/onca/xml");
            
            var urlparams = String.Format("?{0}&Signature={1}",queryString, WebUtility.UrlEncode(signature));
            Uri param = new Uri(urlparams, UriKind.Relative);
            Uri combinedUri = new Uri(baseuri, param);
          
            Task<string> amazonCall = GetAmazonResponse(combinedUri.AbsoluteUri);
            string responseRaw = await amazonCall;

            if (responseRaw != null)
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(responseRaw)))
                {
                    docResponse = XDocument.Load(reader);
                }
            }
            var artistAsin = "";

            if (docResponse != null)
            {
                artistAsin = ProcessAmazonReleases(docResponse);
            }
            Console.WriteLine("Artist Amazon Id: " + artistAsin);
            //return artistAsin;
        }

        public string BuildQueryString(string asinList, string timestamp)
        {
            List<string> paramList = new List<string>();
            paramList.Add("Operation=ItemLookup");
            paramList.Add("ResponseGroup=RelatedItems");
            paramList.Add("Service=AWSECommerceService");
            paramList.Add("Version=2100-01-01");
            paramList.Add("RelationshipType=DigitalMusicPrimaryArtist");
            paramList.Add("AWSAccessKeyId=" + accessKeyId);
            paramList.Add("AssociateTag=" + associateTag);
            paramList.Add("ItemId=" + asinList);
            paramList.Add("Timestamp=" + timestamp);
            var sortedList = SortParametersList(paramList);
            StringBuilder queryStringBuilder = new StringBuilder();
            foreach (string s in sortedList)
            {
                queryStringBuilder.Append(s + "&");   
            }
            queryStringBuilder.Remove(queryStringBuilder.Length - 1,1);
            return queryStringBuilder.ToString();
        }

        public List<string> SortParametersList(List<string> paramList)
        {
            ByteOrderComparer comparer = new ByteOrderComparer();
            paramList.Sort(comparer);
            return paramList;
        }

        public string BuildCanonicalRequest(string querystring)
        {
            var canonicalRequest = new StringBuilder();

            canonicalRequest.AppendFormat("GET\n");
            canonicalRequest.AppendFormat("webservices.amazon.com\n");
            canonicalRequest.AppendFormat("/onca/xml\n");
            canonicalRequest.Append(querystring);

            return canonicalRequest.ToString(); 
        }

        public string GetRequestSignature(string canonicalrequest, string secret)
        {
            
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] canonicalRequestBytes = Encoding.UTF8.GetBytes(canonicalrequest);
            HMAC hmacSha256 = new HMACSHA256(secretKeyBytes);
            byte[] hashBytes = hmacSha256.ComputeHash(canonicalRequestBytes);
            return Convert.ToBase64String(hashBytes);
        }

        public string ProcessAmazonReleases(XDocument data)
        {
            XNamespace aw = data.Root.Name.NamespaceName;
            var artistasin = "";
            IEnumerable<XElement> items = null;
            items = (
                from item in
                    data.ElementOrEmpty(aw, "ItemLookupResponse")
                        .ElementOrEmpty(aw, "Items")
                        .Elements(aw + "Item")
                select item);

            foreach (XElement item in items)
            {
                artistasin =
                    item.ElementOrEmpty(aw, "RelatedItems")
                        .ElementOrEmpty(aw, "RelatedItem")
                        .ElementOrEmpty(aw, "Item")
                        .ElementOrEmpty(aw, "ASIN").Value;
                if (artistasin.Length > 0)
                {
                    return artistasin;
                }
            }
            return artistasin;
        }

        public async Task<string> GetAmazonResponse(string uri)
        {
        using (HttpClient client = new HttpClient())
        using (HttpResponseMessage response = await client.GetAsync(uri))
        using (HttpContent content = response.Content)
        {
            string result = await content.ReadAsStringAsync();
            return result;
        }
        }
    
    }
}
