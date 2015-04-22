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
        public void GetReleases()
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
                    Console.WriteLine(result.Count + "\t" + result.Name + "\t" + result.Description);
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
                    artist.Releases = newReleases.GetAsinDataFromMusicBrainz(artist.Id);
                    var amazonRequest = new AmazonProductAPI();
                    amazonRequest.GetAmazonArtistId(artist);
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
