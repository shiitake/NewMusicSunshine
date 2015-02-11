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
            var results = newReleases.ArtistSearch("Motley Crue");
            if (results.Count > 0)
            {
                Console.WriteLine("Possible Artists:");
                foreach (Artist result in results)
                {
                    Console.WriteLine(result.Name);
                }
            }
            Console.ReadLine();
        }
    }
}
