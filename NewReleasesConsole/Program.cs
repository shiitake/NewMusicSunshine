using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NewReleases.Service.Providers;

namespace NewReleasesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calculating MD5 hash:");
            var rovi = new RoviCloud();
            rovi.CalculateRoviSignature();
        }
    }
}
