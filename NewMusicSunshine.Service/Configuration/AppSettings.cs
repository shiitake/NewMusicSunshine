using System;
using System.Configuration;
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

        public AppSettings()
        {
            _logger = LogManager.GetCurrentClassLogger();
            AmazonAccessKeyId = GetAppSetting<string>("AmazonAccessKeyId");
            AmazonSecretKey = GetAppSetting<string>("AmazonSecretKey");
            AmazonAssociateTag = GetAppSetting<string>("AmazonAssociateTag");
            UserAgent = GetAppSetting<string>("UserAgent");
            RoviApiKey = GetAppSetting<string>("RoviApiKey");
            RoviApiSecret = GetAppSetting<string>("RoviApiSecret");
        }

        private T GetAppSetting<T>(string key)
        {
            var appSetting = ConfigurationManager.AppSettings[key];

            if (String.IsNullOrEmpty(appSetting))
            {
                _logger.Fatal("AppSetting '{0}' does not exist or is empty.", key);
                throw new ConfigurationErrorsException(string.Format("AppSetting '{0}' does not exist or is empty.", key));
            }

            try
            {
                return (T)Convert.ChangeType(appSetting, typeof(T));
            }
            catch
            {
                _logger.Fatal("AppSetting '{0}' must be of type {1}.", key, typeof(T).Name);
                throw new ConfigurationErrorsException(string.Format("AppSetting '{0}' must be of type {1}.", key, typeof(T).Name));
            }
        }

        private T GetAppSetting<T>(string key, T defaultValue)
        {
            var appSetting = ConfigurationManager.AppSettings[key];

            if (String.IsNullOrEmpty(appSetting))
            {
                _logger.Warn("AppSetting '{0}' does not exist or is empty.  Defaulting To: {1}", key, defaultValue);
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(appSetting, typeof(T));
            }
            catch
            {
                _logger.Warn("AppSetting '{0}' must be of type {1}.  Defaulting To: {2}", key, typeof(T).Name, defaultValue);
            }

            return defaultValue;
        }
    }
}
