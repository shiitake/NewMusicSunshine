using NewReleases.Service.Configuration;
using Ninject.Modules;

namespace NewReleases.Service
{
    public class AppModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAppSettings>().To<AppSettings>();
        }
    }
}
