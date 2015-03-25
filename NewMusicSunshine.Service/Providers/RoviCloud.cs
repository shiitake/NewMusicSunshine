using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using NewMusicSunshine.Service.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NewMusicSunshine.Service.Providers
{
    public class RoviCloud
    {
        public static string apikey = "";
        public static string apisecret = "";
        public static string UserAgent = @"NewMusicSunshine/0.1 +https://github.com/shiitake/NewMusicSunshine";

        public async Task GetArtistListFromRovi(string name)
        {
            List<Artist> artistList = new List<Artist>();
            var baseuri = "http://api.rovicorp.com/search/v2.1/music/search?";
            var query = BuildRoviSearchUrl(name);

            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri(baseuri + query);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(baseuri + query);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(content);
                    ProcessArtistList(json);
                }
            }
        }

        private string BuildRoviSearchUrl(string artist)
        {
            var signature = CalculateRoviSignature();
            StringBuilder sb = new StringBuilder();
            sb.Append("apikey=" + apikey + "&");
            sb.Append("sig=" + signature + "&");
            sb.Append("query=" + artist + "&");
            sb.Append("entitytype=" + artist + "&");
            sb.Append("size=10");
            return sb.ToString();
        }

        private void ProcessArtistList(JObject data)
        {
            List<Artist> artistList = new List<Artist>();
            var count = (int)data["searchResponse"]["totalResultCounts"];
            count = (count < 10) ? count : 10;
            for (var i = 0; i < count; i++)
            {
                var artist = new Artist();
                artist.Name = (string)data["searchResponse"]["results"][i]["name"]["name"];
                artist.RoviId = (string) data["searchResponse"]["results"][i]["id"];
                artist.AmgId = (string) data["searchResponse"]["results"][i]["name"]["ids"]["amgPopId"];
                artist.Description = (string) data["searchResponse"]["results"][i]["name"]["headlineBio"];
                artist.DiscographyUrl = (string)data["searchResponse"]["results"][i]["name"]["discographyUri"];
                artistList.Add(artist);
            }
        }
        
        
        public List<Release> GetNewReleasesFromRovi(string id)
        {
            //Todo: build this logic
            return null;
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
            sb.Append("format=json&"); ;
            sb.Append("apikey=" + apikey + "&");
            sb.Append("sig=" + signature);
            return sb.ToString();
        }

        public List<Release> ProcessReleaseData(string data)
        {
            //todo: build this logic
            return null;
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
