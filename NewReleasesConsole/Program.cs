using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NewReleasesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var newReleases = new GetNewReleases();
            var results = newReleases.DiscogArtistSearch("Motley Crue");
            if (results.Count > 0)
            {
                Console.WriteLine("Possible Artists:");
                foreach (Artist result in results)
                {
                    Console.WriteLine(result.Name);
                }
            }
            var releaseList = newReleases.GetNewReleasesFromMusicBrainz();
            if (releaseList.Count > 0)
            {
                Console.WriteLine("Possible Releases:");
                foreach (Release release in releaseList)
                {
                    Console.WriteLine(release.Name);
                    Console.WriteLine(release.Label);
                    Console.WriteLine(release.ReleaseDate);
                    Console.WriteLine(release.ASIN);
                    Console.WriteLine();
                }
            }
            Console.ReadLine();
        }
    }
}
