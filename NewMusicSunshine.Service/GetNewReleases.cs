using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewMusicSunshine.Service.Providers;
using NewMusicSunshine.Service.Models;

namespace NewMusicSunshine.Service
{
    public class GetNewReleases
    {
        private static void GetReleases()
        {
            Console.WriteLine("Enter artist name or enter to exit.");
            Console.Write(">");
            string line;
            line = Console.ReadLine();
            if (line == null)
            {
                return;
            }

            Console.WriteLine("Looking up: " + line);

            var newReleases = new MusicBrainz();
            var results = newReleases.GetArtistListFromMusicBrainz(line);
            //var results = newReleases.DiscogArtistSearch("Motley Crue");
            if (results.Count > 0)
            {
                int count = 1;
                Console.WriteLine("Possible Artists:");
                foreach (Artist result in results)
                {
                    result.Count = count;
                    Console.WriteLine(result.Count + "\t" + result.Name + "\t" + result.Id);
                    count++;
                    if (count == 10)
                    {
                        break;
                    }
                }
                Console.Write("Please choose the correct artist: ");
                int choice;
                bool valid = int.TryParse(Console.ReadLine(), out choice);
                if (valid && choice > 0 && choice <= count)
                {
                    var artist = results.Where(x => choice.Equals(x.Count)).FirstOrDefault();
                    var releaseList = newReleases.GetAsinDataFromMusicBrainz(artist.Id);
                    if (releaseList.Count > 0)
                    {
                        Console.WriteLine("Possible Releases:");
                        foreach (Release release in releaseList)
                        {
                            Console.WriteLine("================");
                            Console.WriteLine("Title: " + release.Name);
                            Console.WriteLine("Label: " + release.Label);
                            Console.WriteLine("Release Date: " + release.ReleaseDate);
                            Console.WriteLine("ASIN: " + release.ASIN);
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("No releases found for " + artist.Name);
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
