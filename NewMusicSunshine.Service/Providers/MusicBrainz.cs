using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NewMusicSunshine.Service.Models;
using System.Xml;
using System.Xml.Linq;
using NewMusicSunshine.Core;



namespace NewMusicSunshine.Service.Providers
{
    public class MusicBrainz
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
                var count = int.Parse(docResponse.Element(aw + "metadata").Element(aw + "release-list").Attribute("count").Value);
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
  //          GetAmazonArtistID(asinList);

            return releaseList;
        }




    }
}
