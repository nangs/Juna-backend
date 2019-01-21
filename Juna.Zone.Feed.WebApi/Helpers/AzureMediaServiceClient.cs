using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Juna.Feed.WebApi.Helpers;

namespace Juna.Feed.WebApi.Helpers
{
    public class AzureMediaServiceClient
    {
        private static async Task<ServiceClientCredentials> GetCredentialsAsync(AzureMediaServices config)
        {
            ClientCredential clientCredential = new ClientCredential(config.AadClientId, config.AadSecret);
            return await ApplicationTokenProvider.LoginSilentAsync(config.AadTenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);
        }

        public static async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(AzureMediaServices config)
        {
            var credentials = await GetCredentialsAsync(config);

            Uri armEndPointUrl = config.ArmEndpoint != null ? new Uri(config.ArmEndpoint) : null;

            return new AzureMediaServicesClient(armEndPointUrl, credentials)
            {
                SubscriptionId = config.SubscriptionId,
            };
        }
    }
}
