﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NewMusicSunshine.Service.Configuration;
using NewMusicSunshine.Service.Models;
using System.Xml;
using System.Xml.Linq;
using Nms.Core;



namespace NewMusicSunshine.Service.Providers
{
    public class MusicBrainz
    {
        public static string UserAgent;
        private string MusicBrainzUrl { get; set; }

        public MusicBrainz()
        {
            var appSettings = new AppSettings();
            UserAgent = appSettings.UserAgent;
            MusicBrainzUrl = appSettings.MusicBrainzUrl;
        }

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
            Uri baseUri = new Uri(MusicBrainzUrl);

            using (var client = new HttpClient())
            {
                client.BaseAddress = baseUri;
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                

                var url = $"/ws/2/artist/?query=artist:{name}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // Send request and get response
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    using (XmlReader reader = XmlReader.Create(response.Content.ReadAsStreamAsync().Result))
                    {
                        docResponse = XDocument.Load(reader);
                    }
                }


            }

            if (docResponse != null)
            {
                //checked release count                
                XNamespace aw = docResponse.Root.Name.NamespaceName;
                var count = int.Parse(docResponse.ElementOrEmpty(aw, "metadata").ElementOrEmpty(aw, "artist-list").AttributeOrEmpty("count").Value);
                if (count > 0)
                {
                    IEnumerable<XElement> artists = null;
                    artists = (
                        from artist in
                            docResponse.ElementOrEmpty(aw, "metadata").ElementOrEmpty(aw,"artist-list").Elements(aw + "artist")
                        select artist
                        );
                    foreach (XElement artist in artists)
                    {
                        var art = new Artist();
                        art.Name = artist.ElementOrEmpty(aw, "name").Value;
                        art.Id = artist.AttributeOrEmpty("id").Value;
                        art.Description = artist.ElementOrEmpty(aw, "disambiguation").Value;
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
            var searchUrl = BuildMBSearchUrl(arid);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(MusicBrainzUrl);
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                var urlPath = $"/ws/2/release/?query={WebUtility.UrlEncode(searchUrl)}";
                var request = new HttpRequestMessage(HttpMethod.Get, urlPath);
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    using (XmlReader reader = XmlReader.Create(response.Content.ReadAsStreamAsync().Result))
                    {
                        docResponse = XDocument.Load(reader);
                    }
                }
            }

            if (docResponse == null) return releaseList;
            //checked release count
            XNamespace aw = docResponse.Root.Name.NamespaceName;
            var count = int.Parse(docResponse.Element(aw + "metadata").Element(aw + "release-list").Attribute("count").Value);
            if (count > 0)
            {
                releaseList = ProcessReleaseData(docResponse);
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
            return releaseList;
        }
    }
}
