using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMusicSunshine.Service.Models
{
    public class Artist
    {
        public string Name { get; set; }
        public List<Release> Releases { get; set; }
        public Release MostRecentRelease
        {
            get { return GetRecentRelease(Releases); }
        }
        public string Title { get; set; }
        public string Country { get; set; }
        public string Id { get; set; }
        public string AmgId { get; set; }
        public string RoviId { get; set; }
        public string Description { get; set; }
        public string DiscographyUrl { get; set; }
        public string Thumb { get; set; }
        public string Uri { get; set; }
        public int Count { get; set; }
        public string Begin { get; set; }
        public string Ended { get; set; }

        public Release GetRecentRelease(List<Release> releases)
        {
            return releases.OrderByDescending(x => x.ReleaseDate).FirstOrDefault();
        }
    }
}
