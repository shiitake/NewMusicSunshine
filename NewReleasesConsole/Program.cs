using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NewMusicSunshine.Service.Providers;

namespace NewReleasesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var artist = "the mountain goats";
            var id = "MN0000480830";
            
            var rovi = new RoviCloud();
            Console.WriteLine("Getting artist List: ");
            rovi.GetArtistListFromRovi(artist).Wait();
            Console.WriteLine("Getting release List: ");
            rovi.GetNewReleasesFromRovi(id).Wait();
            Console.ReadLine();
        }
    }
}
