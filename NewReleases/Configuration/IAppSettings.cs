using System;

namespace NewReleases.Service.Configuration
{
    public interface IAppSettings
    {
        string CronSchedule { get; } 
    }
}
