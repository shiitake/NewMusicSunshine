using System;

namespace NewMusicSunshine.Service.Configuration
{
    public interface IAppSettings
    {
        string AmazonAccessKeyId { get; }
        string AmazonSecretKey { get; }
        string AmazonAssociateTag { get; }
        string UserAgent { get; }
        string RoviApiKey { get; }
        string RoviApiSecret { get; }
    }
}
