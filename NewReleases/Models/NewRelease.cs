using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewReleases.Service.Models
{
    public class NewRelease
    {
        public DateTime ReleaseDate { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
    }
}
