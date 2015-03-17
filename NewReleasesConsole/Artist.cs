using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewReleasesConsole
{
    public class Artist
    {
        public string Name {get; set;}        
        public List<Release> Releases { get; set; }
        public Release MostRecentRelease
        {
            get { return GetRecentRelease(Releases); } 
        }
        public string Title { get; set; }
        public string Country { get; set; }
        public string Id { get; set; }
        public string Thumb { get; set; }
        public string Uri { get; set; }
        public int Count { get; set; }
        public string Begin { get; set; }
        public string Ended { get; set; }

        public Release GetRecentRelease(List<Release> releases )
        {
            return releases.OrderByDescending(x => x.ReleaseDate).FirstOrDefault();
        }
    }

    public class Release
    {
        public string Name { get; set; }

        public DateTime ReleaseDate
        {
            get { return date; } 
        }
        public string Label { get; set; }
        public string Genre { get; set; }
        public string ASIN { get; set; }

        public string StringDate
        {
            set { date = ConvertStringToDate(value); }
        }

        private DateTime date
        {
            get; 
            set; 
        }

        private DateTime ConvertStringToDate(string d)
        {
            switch (d.Length)
            {
                case 0:
                    return DateTime.MinValue;
                case 4:
                    return new DateTime(Int16.Parse(d), 1, 1);
                case 7:
                    var ym = d.Split(new char[] { '-', '/', '.', '\\' });
                    return new DateTime(Int16.Parse(ym[0]), Int16.Parse(ym[1]), 1);
                case 10:
                    var ymd = d.Split(new char[] {'-', '/', '.', '\\'});
                    return new DateTime(Int16.Parse(ymd[0]), Int16.Parse(ymd[1]), Int16.Parse(ymd[2]));
                default:
                    return DateTime.MinValue;
            }
        }
    }
}
