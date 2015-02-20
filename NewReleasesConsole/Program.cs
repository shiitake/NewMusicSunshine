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
            Console.WriteLine("Enter artist name or enter to exit.");
            Console.Write(">");
            string line;
            line = Console.ReadLine();
            if (line == null) { return; }
            
            Console.WriteLine("Looking up: " + line);
            
            var newReleases = new GetNewReleases();
            var results = newReleases.GetArtistListFromMusicBrainz(line);
            //var results = newReleases.DiscogArtistSearch("Motley Crue");
            if (results.Count > 0)
            {
                int count = 1;
                Console.WriteLine("Possible Artists:");
                foreach (Artist result in results)
                {
                    result.Count = count;
                    Console.WriteLine(result.Count + "\t" + result.Name +"\t"+ result.Id);
                    count++;
                }
                Console.Write("Please choose the correct artist: ");
                int choice;
                bool valid = int.TryParse(Console.ReadLine(), out choice);
                if (valid && choice > 0 && choice <= count)
                {
                    var artist = results.Where(x => choice.Equals(x.Count)).FirstOrDefault();
                    var releaseList = newReleases.GetNewReleasesFromMusicBrainz(artist.Id);
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
                    else
                    {
                        Console.WriteLine("No upcoming releases for " + artist.Name);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid choice");
                }
            }
            Console.ReadLine();
        }
    }
}
