﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using NewMusicSunshine.Service.Configuration;
using NewMusicSunshine.Service.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NewMusicSunshine.Service.Providers
{
    public class RoviCloud
    {
        private readonly string _apikey;
        private readonly string _apisecret;
        public static string UserAgent;
        public static string artist = "the mountain goats";
        public static string id = "MN0000480830";

        public RoviCloud()
        {
            var appSettings = new AppSettings();
            _apikey = appSettings.RoviApiKey;
            _apisecret = appSettings.RoviApiSecret;
            UserAgent = appSettings.UserAgent;
        }


        public async Task GetArtistListFromRovi(string name)
        {
            //List<Artist> artistList = new List<Artist>();
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
            sb.Append("apikey=" + _apikey + "&");
            sb.Append("sig=" + signature + "&");
            sb.Append("query=" + artist + "&");
            sb.Append("entitytype=artist&");
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
        
        public async Task GetNewReleasesFromRovi(string id)
        {
            //List<Artist> artistList = new List<Artist>();
            var baseuri = "http://api.rovicorp.com/data/v1.1/name/discography?";
            var query = BuildRoviDiscographyUrl(id);

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
                    ProcessReleaseList(json);
                }
            }
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
            sb.Append("format=json&"); 
            sb.Append("apikey=" + _apikey + "&");
            sb.Append("sig=" + signature);
            return sb.ToString();
        }

        public void ProcessReleaseList(JObject data)
        {
            List<Release> releaseList = new List<Release>();
            var count = (int) data["view"]["total"];
            //count = (count < 10) ? count : 10;
            for (var i = 0; i < count; i++)
            {
                var release = new Release();
                release.Name = (string) data["discography"][i]["title"];
                release.RoviId = (string) data["discography"][i]["ids"]["albumId"];
                release.AmgId = (string) data["discography"][i]["ids"]["amgPopId"];
                release.StringDate = (string) data["discography"][i]["year"];
                release.Type = (string) data["discography"][i]["type"];
                release.Label = (string) data["discography"][i]["label"];
                releaseList.Add(release);
            }
        }

        public string CalculateRoviSignature()
        {
            var input = _apikey + _apisecret + UnixTimeNow();
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
            return timeSpan.ToString();
        }

    }
}
