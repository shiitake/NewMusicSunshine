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
                foreach (XElement release in releases)
                {
                    var rel = new Release();
                    rel.Name = release.ElementOrEmpty(aw, "title").Value;
                    rel.StringDate = release.ElementOrEmpty(aw, "date").Value;
                    rel.Label = release.ElementOrEmpty(aw, "label-info-list").ElementOrEmpty(aw, "label-info")
                            .ElementOrEmpty(aw, "label")
                            .ElementOrEmpty(aw, "name").Value;
                    rel.ASIN = release.ElementOrEmpty(aw, "asin").Value ?? "";
                    asinList += (rel.ASIN.Length > 0) ? rel.ASIN + "%2C" : "";
                    releaseList.Add(rel);
                }
            }
            var artistAsin = GetAmazonArtistID(asinList);
            return releaseList;
        }

        public string GetAmazonArtistID(string asinList)
        {
            string accessKeyId = "";
            string secretKey = "";
            string associateTag = "newmussun-20";

            string operation = "ItemLookup";
            DateTime now = DateTime.UtcNow;
            string timestamp = now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string signMe = operation + timestamp;
            byte[] bytesToSign = Encoding.UTF8.GetBytes(signMe);

            // sign the data
            var signature = GetRequestSignature(secretKey, bytesToSign);

            XDocument docResponse = null;
            Uri baseuri = new Uri("http://ecs.amazonaws.com/onca/xml");
            var urlparams = String.Format("?AWSAccessKeyId={0}&AssociateTag={1}&Operation={2}&ItemId={3}&RelationshipType=DigitalMusicPrimaryArtist&ResponseGroup=RelatedItems&Service=AWSECommerceService&Timestamp={4}&Version=2100-01-01&Signature={5}",
                            accessKeyId, associateTag, operation, asinList, WebUtility.UrlEncode(timestamp), WebUtility.UrlEncode(signature));
            Uri param = new Uri(urlparams, UriKind.Relative);
            Uri combinedUri = new Uri(baseuri, param);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(HackedUri.Create(combinedUri.AbsoluteUri));
            req.Method = "GET";
            req.UserAgent = UserAgent;
            
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (XmlReader reader = XmlReader.Create(resp.GetResponseStream()))
                {
                    docResponse = XDocument.Load(reader);
                }
            }
            var artistAsin = "";

            if (docResponse != null)
            {
                artistAsin = ProcessAmazonReleases(docResponse);
            }
            return artistAsin;
        }

        public string GetRequestSignature(string key, byte[] unsigned)
        {
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(key);
            HMAC hmacSha256 = new HMACSHA256(secretKeyBytes);
            byte[] hashBytes = hmacSha256.ComputeHash(unsigned);
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
                if (artistasin != null)
                {
                    return artistasin;
                }
            }
            return artistasin;
        }
    }
}
