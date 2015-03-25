using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using NewReleases.Service.Models;

namespace NewReleases.Service.Providers
{
    public class RoviCloud
    {
        public static string apikey = "";
        public static string apisecret = "";
        public static string UserAgent = @"NewMusicSunshine/0.1 +https://github.com/shiitake/NewMusicSunshine";

        private string BuildRoviSearchUrl(string artist)
        {
            var signature = CalculateRoviSignature();
            StringBuilder sb = new StringBuilder();
            sb.Append("apikey=" + apikey + "&");
            sb.Append("sig=" + signature + "&");
            sb.Append("query=" + artist + "&");
            sb.Append("entitytype=" + artist + "&");
            sb.Append("size=1");
            return sb.ToString();
        }

        private string BuildRoviDiscographyUrl(string id)
        {
            var signature = CalculateRoviSignature();
            StringBuilder sb = new StringBuilder();
            sb.Append("nameid=" + id + "&");
            sb.Append("count=0&");
            sb.Append("offset=0&");
            sb.Append("country=US&");
            sb.Append("language=en&");
            sb.Append("format=json&");;
            sb.Append("apikey=" + apikey + "&");
            sb.Append("sig=" + signature);
            return sb.ToString();
        }

        public List<Artist> GetArtistListFromRovi(string name)
        {
            List<Artist> artistList = new List<Artist>();
            var baseuri = "http://api.rovicorp.com/search/v2.1/music/search?";
            var query = BuildRoviSearchUrl(name);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(baseuri + query);
            req.Method = "GET";
            req.UserAgent = UserAgent;

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (Stream receiveStream = resp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
                    var responseString = reader.ReadToEnd();
                }
            }
        }

        public List<Release> GetNewReleasesFromRobi(string id)
        {
            
        }

        public List<Release> ProcessReleaseData(string data)
        {
            
        }
        
        public string CalculateRoviSignature()
        {
            var input = apikey + apisecret + UnixTimeNow();
            Console.WriteLine(input);
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inpuBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inpuBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }

        public string UnixTimeNow()
        {
            var timeSpan = (Int32)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            Console.WriteLine("Seconds: " + timeSpan);
            return timeSpan.ToString();
        }

    }
}
