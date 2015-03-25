using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NewReleases.Service.Providers
{
    public class AllMusic
    {
        public void Execute()
        {
            var url = "http://www.allmusic.com/newreleases/all";
            var webGet = new HtmlWeb();
            var document = webGet.Load(url);
            var newReleases = from table in document.DocumentNode.SelectNodes("//tbody").Cast<HtmlNode>()
                              from row in table.SelectNodes("tr").Cast<HtmlNode>()
                              from cell in row.SelectNodes("th|td").Cast<HtmlNode>()
                              select new { Table = table.Id, CellText = cell.InnerText.Trim() };
            //where release.Name != "a" 
            //&& release.InnerText.Trim().Length > 0
            //&& release.Name != "span" 
            //&& release.Attributes["class"] != null
            //select release;

            foreach (var cell in newReleases)
            {
                Console.WriteLine("{0}:{1}", cell.Table, cell.CellText);
            }

            //Console.WriteLine("Artists" + "\t" + "Album" + "\t" + "Genre");
            //string artist = "artist";
            //string album;
            //string genre;
            //foreach (var release in newReleases)
            //{
            //    if (release.Attributes["class"].Value == "artist")
            //    {
            //        artist = release.InnerText.Trim();
            //        Console.Write(artist + "\t");
            //    }
            //    else if (release.Attributes["class"].Value == "album")
            //    {
            //        album = release.InnerText.Trim();
            //        Console.Write(album + "\t");
            //    }
            //    else if (release.Attributes["class"].Value == "genre")
            //    {
            //        genre = release.InnerText.Trim();
            //        Console.WriteLine(genre);
            //    }
            //}
            Console.ReadLine();
        }
    }
}
