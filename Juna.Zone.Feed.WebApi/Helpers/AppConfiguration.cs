using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juna.Feed.WebApi.Helpers
{
    public class AppConfiguration : IAppConfiguration
    {
        public AppConfiguration()
        {
            AppSettings = new AppSettings();
            AzureMediaServices = new AzureMediaServices();
        }

        public Logging Logging { get; set; }
        public AppSettings AppSettings { get; set; }
        public AzureMediaServices AzureMediaServices { get; set; }
    }

    public class LogLevel
    {
        public string Default { get; set; }
        public string System { get; set; }
        public string Microsoft { get; set; }
    }

    public class Logging
    {
        public bool IncludeScopes { get; set; }
        public LogLevel LogLevel { get; set; }
    }
    public class AppSettings
    {
        public string AzureWebJobsStorage { get; set; }
        public string CosmosdbEndpointUrl { get; set; }
        public string CosmosdbPrimaryKey { get; set; }
        public string CosmosdbDatabaseName { get; set; }
        public string StreamAccessKey { get; set; }
        public string StreamSecret { get; set; }
        public string FootballFeedName { get; set; }
        public string SocialDataCollectionName { get; set; }
        public string ImageUploadStorageFolder { get; set; }
        public string ThumbnailServiceApiUrl { get; set; }
        public string ThumbnailServiceApiKeyName { get; set; }
        public string ThumbnailServiceApiKeyValue { get; set; }
        public long ThumbnailHeight { get; set; }
        public long ThumbnailWidth { get; set; }
        public string StorageKeyName { get; set; }
        public string StorageKeyValue { get; set; }
        public string FCMApplicationId { get; set; }
        public string FCMSenderId { get; set; }
        public string FCMUrl { get; set; }
        public string OidClaim { get; set; }
        public string NewsFeedApiKey { get; set; }
        public string BoardActivationNotifierKey { get; set; }
        public string FootballDataApiKey { get; set; }
        public string JunaRestApiEndpoint { get; set; }
    }

    public class AzureMediaServices
    {
        public string AadClientId { get; set; }
        public string AadEndpoint { get; set; }
        public string AadSecret { get; set; }
        public string AadTenantId { get; set; }
        public string AccountName { get; set; }
        public string ArmAadAudience { get; set; }
        public string ArmEndpoint { get; set; }
        public string Region { get; set; }
        public string ResourceGroup { get; set; }
        public string SubscriptionId { get; set; }
    }
}
