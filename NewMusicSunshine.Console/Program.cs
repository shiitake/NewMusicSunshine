using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NewMusicSunshine.Service;
using NewMusicSunshine.Service.Providers;

namespace NewMusicSunshine.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //var rovi = new RoviCloud();
            //Console.WriteLine("Getting artist List: ");
            //rovi.GetArtistListFromRovi(artist).Wait();
            //Console.WriteLine("Getting release List: ");
            //rovi.GetNewReleasesFromRovi(id).Wait();
            var releases = new GetNewReleases();
            releases.GetReleases();
            System.Console.ReadLine();

        }
    }
}
