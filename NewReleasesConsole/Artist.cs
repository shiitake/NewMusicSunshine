using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewReleasesConsole
{
    public class Artist
    {
        public string Name { get; set; }
        public List<Release> Releases { get; set; }
        public Release MostRecentRelease
        {
            get { return GetRecentRelease(Releases); } 
        }

        public Release GetRecentRelease(List<Release> releases )
        {
            return releases.OrderByDescending(x => x.ReleaseDate).FirstOrDefault();
        }
    }

    public class Release
    {
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Label { get; set; }
        public string Genre { get; set; }
    }
}
