using System;
using System.Configuration;
using System.IO;
using Microsoft.Extensions.Configuration;
using NLog;

namespace NewMusicSunshine.Service.Configuration
{
    public class AppSettings : IAppSettings
    {
        private readonly Logger _logger;
        public string AmazonAccessKeyId { get; set; }
        public string AmazonSecretKey { get; set; }
        public string AmazonAssociateTag { get; set; }
        public string UserAgent { get; set; }
        public string RoviApiKey { get; set; }
        public string RoviApiSecret { get; set; }
        public string MusicBrainzUrl { get; set; }
        private IConfiguration Configuration { get; set; }

        public AppSettings()
        {
            _logger = LogManager.GetCurrentClassLogger();
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = configurationBuilder.Build();
            AmazonAccessKeyId = Configuration["Amazon:AccessKeyId"];
            AmazonSecretKey = Configuration["Amazon:SecretKey"];
            AmazonAssociateTag = Configuration["Amazon:AssociateTag"];
            UserAgent = Configuration["UserAgent"];
            RoviApiKey = Configuration["Rovi:ApiKey"];
            RoviApiSecret = Configuration["Rovi:ApiSecret"];
            MusicBrainzUrl = Configuration["MusicBrainz:Url"];

        }
    }
}
