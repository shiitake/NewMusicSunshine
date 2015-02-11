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
using DiscogsNet;
using DiscogsNet.Api;
using DiscogsNet.Model.Search;
using DiscogsNet.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace NewReleasesConsole
{
    public class GetNewReleases
    {
        public static string ConsumerKey = "KsvRkSIfcmSsItDhSoZL";
        public static string ConsumerSecret = "IoyxbThdqEPLjGThJaibUaeBwIsNlrUF";
        public static string RequestTokenURL = @"http://api.discogs.com/oauth/request_token";
        public static string AuthorizeURL = @"http://www.discogs.com/oauth/authorize";
        public static string AccessTokenURL = @"http://api.discogs.com/oauth/access_token";
        public static string UserAgent = @"NewMusicSunshine/0.1 +https://github.com/shiitake/NewMusicSunshine";

        public List<Artist> ArtistSearch(string artist)
        {
            var authorization = @"Discogs key=" + ConsumerKey + ",secret=" + ConsumerSecret;
            var url = BuildSearchUrl(artist);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.UserAgent = UserAgent;
            req.Headers.Add("Authorization", authorization);

            using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse())
            {
                using (Stream getStream = resp.GetResponseStream())
                {
                    StreamReader readStream = new StreamReader(getStream, true);
                    var result = readStream.ReadToEnd();

                    JObject json = JObject.Parse(result);
                    List<JToken> searchResults = json["results"].Children().ToList();
                    List<Artist> artistList = new List<Artist>();
                    foreach (JToken searchResult in searchResults)
                    {
                        Artist artistResult = JsonConvert.DeserializeObject<Artist>(searchResult.ToString());
                        artistList.Add(artistResult);
                    }
                    return artistList;
                }
            }
        }

        public string BuildSearchUrl(string artist)
        {
            return String.Format("https://api.discogs.com/database/search?q={0}&type=artist", artist);
        }
    }
}
