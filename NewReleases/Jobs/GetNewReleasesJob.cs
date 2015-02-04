using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewReleases.Service.Configuration;
using NewReleases.Service.Models;
using Ninject.Extensions.Logging;
using Quartz;
using HtmlAgilityPack;


namespace NewReleases.Service.Jobs
{
    public class GetNewReleasesJob : IJob
    {
        private readonly ILogger _logger;
        private readonly IAppSettings _appSettings;

        public GetNewReleasesJob(ILogger logger, IAppSettings appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Start();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error attempting to start the Get New Releases Job");
                throw;
            }
            finally
            {
                var jobTime = context.NextFireTimeUtc;
                if (jobTime != null)
                    _logger.Info("Job Scheduled to run again at {0}", jobTime.Value.ToLocalTime().ToString("MM-dd-yyyy HH:mm"));
            }
        }

        public void Start()
        {
            _logger.Debug("Get New Releases Job Starting.");
            var releaseList = GetReleaseList();
            _logger.Debug("Get New Releases Job Finished.");
        }

        public List<NewRelease> GetReleaseList()
        {
            var url = "http://www.allmusic.com/newreleases/all";
            var webGet = new HtmlWeb();
            var document = webGet.Load(url);
            var newReleases = from release in document.DocumentNode.SelectNodes("//tbody/tr").Descendants()
                              where release.Name != "a"
                              && release.Name != "span"
                              && release.Attributes["class"] != null
                              select release;
            
            var newRelease = new NewRelease();
            var releaseList = new List<NewRelease>();
            newRelease.ReleaseDate = DateTime.Now;
            foreach (var release in newReleases)
            {
                if (release.Attributes["class"].Value == "artist")
                {
                    newRelease.Artist = release.InnerText.Trim();
                }
                else if (release.Attributes["class"].Value == "album")
                {
                 newRelease.Album = release.InnerText.Trim();
             
                }
                else if (release.Attributes["class"].Value == "genre")
                {
                    newRelease.Genre = release.InnerText.Trim();
                    releaseList.Add(newRelease);
                }
            }
            return releaseList;
        }
    }
}
