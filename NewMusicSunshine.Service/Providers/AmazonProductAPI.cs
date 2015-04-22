using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NewMusicSunshine.Service.Configuration;
using NewMusicSunshine.Service.Models;
using System.Xml;
using System.Xml.Linq;
using NewMusicSunshine.Core;

namespace NewMusicSunshine.Service.Providers
{
    public class AmazonProductAPI
    {
        private readonly string _accessKeyId;
        private readonly string _secretKey;
        private readonly string _associateTag;
        public static string UserAgent;

        public AmazonProductAPI()
        {
            var appSettings = new AppSettings();
            _accessKeyId = appSettings.AmazonAccessKeyId;
            _secretKey = appSettings.AmazonSecretKey;
            _associateTag = appSettings.AmazonAssociateTag;
            UserAgent = appSettings.UserAgent;
        }

        public async void GetAmazonArtistId(Artist artist)
        {
            var asinList = new string[5];
            var count = 0;
            if (artist.Releases.Count > 0)
            {
                foreach (Release release in artist.Releases)
                {
                    if (release.ASIN.Length > 0 && count < 5)
                    {
                        asinList[++count - 1] = release.ASIN;
                    }
                } 
            }
            
            DateTime now = DateTime.UtcNow;
            string timestamp = now.ToString("yyyy-MM-ddTHH:mm:ss.000Z");

            // build query
            var queryString = BuildArtistIdQueryString(WebUtility.UrlEncode(string.Join(",", asinList)), WebUtility.UrlEncode(timestamp));
            var canonicalRequest = BuildCanonicalRequest(queryString);

            // sign the data
            var signature = GetRequestSignature(canonicalRequest, _secretKey);

            XDocument docResponse = null;
            Uri baseuri = new Uri("http://webservices.amazon.com/onca/xml");

            var urlparams = String.Format("?{0}&Signature={1}", queryString, WebUtility.UrlEncode(signature));
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
                artistAsin = ProcessAmazonArtistId(docResponse);
            }
            if (artistAsin.Length > 0)
            {
                Console.WriteLine("Artist Amazon Id: " + artistAsin);
                GetAmazonArtistNameById(artistAsin);
            }
            else
            {
                Console.WriteLine("Unable to find Amazon Artist ID");
            }
        }

        public async void GetAmazonArtistNameById(string id)
        {
            DateTime now = DateTime.UtcNow;
            string timestamp = now.ToString("yyyy-MM-ddTHH:mm:ss.000Z");

            // build query
            var queryString = BuildArtistNameQueryString(id, WebUtility.UrlEncode(timestamp));
            var canonicalRequest = BuildCanonicalRequest(queryString);

            // sign the data
            var signature = GetRequestSignature(canonicalRequest, _secretKey);

            XDocument docResponse = null;
            Uri baseuri = new Uri("http://webservices.amazon.com/onca/xml");

            var urlparams = String.Format("?{0}&Signature={1}", queryString, WebUtility.UrlEncode(signature));
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
            var artistName = "";

            if (docResponse != null)
            {
                artistName = ProcessAmazonArtistName(docResponse);
            }
            if (artistName.Length > 0)
            {
                Console.WriteLine("Artist name: " + artistName);
                GetAmazonReleaseByArtist(artistName);
            }
            else
            {
                Console.WriteLine("Unable to find Amazon Artist ID");
            }
        }

        public async void GetAmazonReleaseByArtist(string artist)
        {
            DateTime now = DateTime.UtcNow;
            string timestamp = now.ToString("yyyy-MM-ddTHH:mm:ss.000Z");
            
            // build query
            var queryString = BuildReleaseQueryString(HttpUtility.UrlPathEncode(artist), WebUtility.UrlEncode(timestamp));
            var canonicalRequest = BuildCanonicalRequest(queryString);

            // sign the data
            var signature = GetRequestSignature(canonicalRequest, _secretKey);

            XDocument docResponse = null;
            Uri baseuri = new Uri("http://webservices.amazon.com/onca/xml");

            var urlparams = String.Format("?{0}&Signature={1}", queryString, WebUtility.UrlEncode(signature));
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
            var releaseList = new List<Release>();

            if (docResponse != null)
            {
                releaseList = ProcessAmazonReleases(docResponse);
            }
            if (releaseList.Count > 0)
            {
                Console.WriteLine(releaseList.Count + " release(s) found");
                foreach (var release in releaseList)
                {
                    Console.WriteLine("Name: " + release.Name + "\tType: " + release.Type + "\tDate: " +release.ReleaseDate);
                }
            }
            else
            {
                Console.WriteLine("No releases found for " + artist);
            }
        }

        public string BuildArtistIdQueryString(string asinList, string timestamp)
        {
            List<string> paramList = new List<string>();
            paramList.Add("Operation=ItemLookup");
            paramList.Add("ResponseGroup=RelatedItems");
            paramList.Add("Service=AWSECommerceService");
            paramList.Add("Version=2100-01-01");
            paramList.Add("RelationshipType=DigitalMusicPrimaryArtist");
            paramList.Add("AWSAccessKeyId=" + _accessKeyId);
            paramList.Add("AssociateTag=" + _associateTag);
            paramList.Add("ItemId=" + asinList);
            paramList.Add("Timestamp=" + timestamp);
            var sortedList = SortParametersList(paramList);
            StringBuilder queryStringBuilder = new StringBuilder();
            foreach (string s in sortedList)
            {
                queryStringBuilder.Append(s + "&");
            }
            queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);
            return queryStringBuilder.ToString();
        }

        public string BuildArtistNameQueryString(string id, string timestamp)
        {
            List<string> paramList = new List<string>();
            paramList.Add("Service=AWSECommerceService");
            paramList.Add("Operation=ItemLookup");
            paramList.Add("ResponseGroup=Small");
            paramList.Add("Version=2100-01-01");
            paramList.Add("AWSAccessKeyId=" + _accessKeyId);
            paramList.Add("AssociateTag=" + _associateTag);
            paramList.Add("ItemId=" + id);
            paramList.Add("Timestamp=" + timestamp);
            var sortedList = SortParametersList(paramList);
            StringBuilder queryStringBuilder = new StringBuilder();
            foreach (string s in sortedList)
            {
                queryStringBuilder.Append(s + "&");
            }
            queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);
            return queryStringBuilder.ToString();
        }
        
        public string BuildReleaseQueryString(string artist, string timestamp, int page = 0)
        {
           List<string> paramList = new List<string>();
            paramList.Add("Service=AWSECommerceService");
            paramList.Add("Operation=ItemSearch");
            paramList.Add("ResponseGroup=Medium");
            paramList.Add("Version=2100-01-01");
            paramList.Add("SearchIndex=Music");
            //only returns vinyl
            //paramList.Add("BrowseNode=2334136011");
            paramList.Add("BrowseNode=5174");
            paramList.Add("Sort=-releasedate");
            paramList.Add("AWSAccessKeyId=" + _accessKeyId);
            paramList.Add("AssociateTag=" + _associateTag);
            paramList.Add("Artist=" + artist);
            paramList.Add("Timestamp=" + timestamp);
            if (page > 0)
            {
                paramList.Add("ItemPage=" + page);
            }
            var sortedList = SortParametersList(paramList);
            StringBuilder queryStringBuilder = new StringBuilder();
            foreach (string s in sortedList)
            {
                queryStringBuilder.Append(s + "&");
            }
            queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1);
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

        public string ProcessAmazonArtistId(XDocument data)
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

        public List<Release> ProcessAmazonReleases(XDocument data)
        {
            XNamespace aw = data.Root.Name.NamespaceName;
            var releaseList = new List<Release>();
            IEnumerable<XElement> items = null;
            var count = data.ElementOrEmpty(aw, "ItemSearchResponse")
                .ElementOrEmpty(aw, "Items")
                .ElementOrEmpty(aw, "TotalResults").Value;
            if (Int16.Parse(count) > 0)
            {
                items = (
                    from item in
                        data.ElementOrEmpty(aw, "ItemSearchResponse")
                        .ElementOrEmpty(aw, "Items")
                        .Elements(aw + "Item")
                    select item);
                foreach (XElement item in items)
                {
                    var release = new Release();
                    release.ASIN = item.ElementOrEmpty(aw, "ASIN").ValueOrEmpty();
                    release.Name = item.ElementOrEmpty(aw, "ItemAttributes")
                        .ElementOrEmpty(aw, "Title").ValueOrEmpty();
                    release.StringDate = item.ElementOrEmpty(aw, "ItemAttributes")
                        .ElementOrEmpty(aw, "ReleaseDate").ValueOrEmpty();
                    release.Label = item.ElementOrEmpty(aw, "ItemAttributes")
                        .ElementOrEmpty(aw, "Label").ValueOrEmpty();
                    release.Type = item.ElementOrEmpty(aw, "ItemAttributes")
                        .ElementOrEmpty(aw, "Binding").ValueOrEmpty();
                    release.UPC = item.ElementOrEmpty(aw, "ItemAttributes")
                        .ElementOrEmpty(aw, "UPC").ValueOrEmpty();
                    release.Image = item.ElementOrEmpty(aw, "LargeImage")
                        .ElementOrEmpty(aw, "URL").ValueOrEmpty();
                    releaseList.Add(release);
                }
            }
                return releaseList;            
        }

        public string ProcessAmazonArtistName(XDocument data)
        {
            XNamespace aw = data.Root.Name.NamespaceName;
            var artistName = "";
            IEnumerable<XElement> items = null;
            items = (
                from item in
                    data.ElementOrEmpty(aw, "ItemLookupResponse")
                        .ElementOrEmpty(aw, "Items")
                        .Elements(aw + "Item")
                select item);
            foreach (XElement item in items)
            {
                artistName =
                    item.ElementOrEmpty(aw, "ItemAttributes")
                        .ElementOrEmpty(aw, "Title").Value;
                if (artistName.Length > 0)
                {
                    return artistName;
                }
            }
            return artistName;
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
