using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMusicSunshine.Service.Models
{
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
        public string Type { get; set; }
        public string RoviId { get; set; }
        public string AmgId { get; set; }
        public string UPC {get; set; }
        public string Image { get; set; }

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
                    string corrected = d.Replace("??", "01");
                    var ymd = corrected.Split(new char[] { '-', '/', '.', '\\' });
                    return new DateTime(Int16.Parse(ymd[0]), Int16.Parse(ymd[1]), Int16.Parse(ymd[2]));
                default:
                    return DateTime.MinValue;
            }
        }
    }
}
