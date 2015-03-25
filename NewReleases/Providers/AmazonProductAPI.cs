using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NewReleases.Service.Models;
using System.Xml;
using System.Xml.Linq;
using NewMusicSunshine.Core;

namespace NewReleases.Service.Providers
{
    public class AmazonProductAPI
    {
        string accessKeyId = "";
        string secretKey = "";
        string associateTag = "newmussun-20";
        public static string UserAgent = @"NewMusicSunshine/0.1 +https://github.com/shiitake/NewMusicSunshine";

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
