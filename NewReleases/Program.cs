using System;
using NewReleases.Service.Configuration;
using NewReleases.Service.Jobs;
using NLog;
using Quartz;
using Topshelf;
using Topshelf.Ninject;
using Topshelf.Quartz;
using Topshelf.Quartz.Ninject;

namespace NewReleases.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            IAppSettings appSettings = new AppSettings();

            try
            {
                HostFactory.Run(c =>
                {
                    c.UseNLog();
                    c.UseNinject(new AppModule());
                    c.UseQuartzNinject();
                    c.RunAsLocalSystem();

                    c.ScheduleQuartzJobAsService(q =>
                        q.WithJob(() =>
                            JobBuilder.Create<GetNewReleasesJob>()
                                .WithDescription("Get New Releases Job")
                                .Build())
                            .AddTrigger(() =>
                                TriggerBuilder.Create()
                                    .WithCronSchedule(
                                        appSettings.CronSchedule,
                                        cron => cron.WithMisfireHandlingInstructionDoNothing())
                                    .Build()));
                });
            }
            catch (Exception ex)
            {
                logger.Fatal("Get New Releases Job service failed to start. {0}", ex.Message);
                throw;
            }
            
        }
    }
}
