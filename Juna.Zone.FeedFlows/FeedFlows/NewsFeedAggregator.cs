using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using System.Linq;
using Juna.FeedFlows.Infrastructure;
using Juna.Feed.DomainModel;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Juna.FeedFlows.Util;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;

namespace Juna.FeedFlows.FeedFlows
{
	[StorageAccount("AzureWebJobsStorage")]
	public static class NewsFeedAggregator
	{
		// todo: Tweak this term to include the other three prominent leagues as well as the World Cup
		private const string searchTerm = "English Premiere League";

        [FunctionName("NewsFeedAggregator")]
        public static async System.Threading.Tasks.Task RunAsync(
            [TimerTrigger("0 30 11 * * *")]TimerInfo myTimer,
            TraceWriter log,
            ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                            .SetBasePath(context.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

            var appConfig = new AppSettings();
            config.GetSection("Values").Bind(appConfig);

            log.Info($"NewsFeedAggregator executed at: {DateTime.Now}");

            try
            {
                var cognitiveApiAccessKey = config.GetSection("Values")["CognitiveApiAccessKey"];
                 

                //searchService = new BingSearchService(searchTerm, cognitiveApiAccessKey, log) { UriBase = config.GetSection("Values")["CognitiveApiUrl"] };
                //log.Info("Searching news for: " + searchTerm);
                var bingClient = new HttpClient { BaseAddress = new Uri("https://api.cognitive.microsoft.com/") };
                bingClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", cognitiveApiAccessKey);


                //var result = searchService.BingNewsSearch();
                // log.Info("Searching news for: " + searchTerm);
                var result = bingClient.GetStringAsync("bing/v7.0/news/search?mkt=en-GB&count=100&freshness=Month&q=English%20Premiere%20League").Result;


                // todo: Ugly json parsing. Improve this.
                log.Info("\n Getting most recent links \n");
                //    var data = JObject.Parse(result.jsonResult);
                var data = JObject.Parse(result);

                IEnumerable<FeedItem> recentItemsUrl = from i in data["value"]
                                                       select new NewsFeedItem
                                                       {
                                                           Title = (string)i["name"],
                                                           Url = (string)i["url"],
                                                           Thumbnail = (i["image"] == null) ? null : new Image
                                                           (
                                                               url: (string)i["image"]["thumbnail"]["contentUrl"],
                                                               height: (int)i["image"]["thumbnail"]["height"],
                                                               width: (int)i["image"]["thumbnail"]["width"]
                                                           ),
                                                           Source = ((IEnumerable<dynamic>)i["provider"]).Select(j => (string)j["name"]).FirstOrDefault(),
                                                           DatePublished = DateTime.UtcNow,
                                                           //todo:  Date parsing is not working for some reason. Need to investigate
                                                           //DatePublished = string.IsNullOrEmpty((string)i["datePublished"])
                                                           //? DateTime.UtcNow
                                                           //: DateTime.Parse((string)i["datePublished"], null, System.Globalization.DateTimeStyles.RoundtripKind),
                                                           Summary = (string)i["description"],
                                                           Tags = (i["about"] == null) ? null : ((IEnumerable<dynamic>)i["about"]).Select(a => (string)a["name"]).AsEnumerable().ToList()
                                                       };

                //Don't use newsfeed articles without thumbnail
                var recentItems = recentItemsUrl.Where(n => n.Thumbnail != null && !n.Summary.Contains("\"")).ToArray();
                var junaApi = config.GetSection("Values")["JunaRestApiEndpoint"];
                //appConfig.NewsFeedApiKey = config.GetSection("Values")["ApiKey"];

                var client = new HttpClient
                {
                    BaseAddress = new Uri(junaApi)
                };

                //change to http request 

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                //post as json to web api feed items
                //system to system authenticaon in Azure AD B2C
                //whole team need to writes bot
                //when post feed item then actor JunaUser Football (dummy user in database) 
                string jsonRecentItems = JsonConvert.SerializeObject(recentItems);

                //ConfidentialClientApplication daemonClient = DaemonClientSetup(appConfig);
                //AuthenticationResult authResult = await daemonClient.AcquireTokenForClientAsync(new string[] { appConfig.Scope });
                var newsFeedApiKey = appConfig.NewsFeedApiKey;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, appConfig.JunaCreateFeedItems);

                request.Headers.Add("News-Feed-Api-Key", newsFeedApiKey);
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                request.Content = new StringContent(jsonRecentItems, Encoding.UTF8, "application/json");

                //var response2 = client.PostAsJsonAsync<FeedItem[]>(junaApi, recentItems).Result;
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Created)
                    log.Info($"Inserted { recentItemsUrl.Count() } news items into the queue");
                else
                    log.Error($"Failed with status code [{response.StatusCode}]");
            }
            catch (Exception e)
            {
                log.Error($"NewsfeedAggregator errored out with message [{e.Message}]");
            }
        }

        private static ConfidentialClientApplication DaemonClientSetup(AppSettings appSettings)
        {
            ConfidentialClientApplication daemonClient = new ConfidentialClientApplication(
                appSettings.ClientId
                ,String.Format(appSettings.AuthorityFormat, appSettings.TenantId), 
                appSettings.RedirectUri, 
                new ClientCredential(appSettings.ClientSecret), null, new TokenCache());

            return daemonClient;
        }
    public class Thumbnail
    {
        public string ContentUrl { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }

    }
}
