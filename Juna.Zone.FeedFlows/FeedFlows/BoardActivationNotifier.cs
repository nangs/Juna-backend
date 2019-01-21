using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using System.Linq;
using Juna.FeedFlows.Infrastructure;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Juna.Feed.DomainModel;
using Juna.FeedFlows.Util;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;

namespace Juna.FeedFlows
{
    [StorageAccount("AzureWebJobsStorage")]
    public class BoardActivationNotifier
    {
        [FunctionName("BoardActivationNotifier")]
        public async static System.Threading.Tasks.Task RunAsync(
            [TimerTrigger("0 */1 * * * *")]TimerInfo myTimer,
            TraceWriter log,
            ExecutionContext context)
        {
            log.Info($"BoardActivationNotifier executed at: {DateTime.Now}");

            var config = new ConfigurationBuilder()
                            .SetBasePath(context.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

            var appConfig = new AppSettings();
            config.GetSection("Values").Bind(appConfig);

            var notifierClient = new HttpClient { BaseAddress = new Uri(appConfig.JunaRestApiEndpoint) };

            var notifierKey = appConfig.BoardActivationNotifierKey;
            var request = new HttpRequestMessage(HttpMethod.Get, $"{appConfig.JunaRestApiEndpoint}/boards/date/{DateTime.UtcNow.Date.ToString().Split(' ')[0]}");
            request.Headers.Add("Activation-Notifier-Api-Key", notifierKey);
            HttpResponseMessage response = await notifierClient.SendAsync(request);
            var boardsToday = JsonConvert.DeserializeObject<List<Board>>(response.Content.ReadAsStringAsync().Result);
            log.Info("ran");
            var timeNow = DateTime.UtcNow.ToString().Split(' ')[1];

            
#pragma warning disable AvoidAsyncVoid // Avoid async void
            boardsToday.ForEach(async board =>
#pragma warning restore AvoidAsyncVoid // Avoid async void
            {
                //if (board.StartTime == timeNow)
                {
                    var liveData = new
                    {
                        liveEventType = "board",
                        foreignId = board.BoardEvent.ForeignId,
                        boardTopic = $"football-match-{board.BoardEvent.ForeignId}",
                        liveDataType = "boardActivated",
                        data = false
                    };

                    var activateReq = new HttpRequestMessage(HttpMethod.Post, $"{appConfig.JunaRestApiEndpoint}/boards/live");
                    activateReq.Headers.Add("Activation-Notifier-Api-Key", notifierKey);
                    activateReq.Content = new StringContent(JsonConvert.SerializeObject(liveData), Encoding.UTF8, "application/json");
                    HttpResponseMessage resp = await notifierClient.SendAsync(activateReq);
                }
                //if (board.EndTime == timeNow)
                {
                    var liveData = new
                    {
                        liveEventType = "board",
                        foreignId = board.BoardEvent.ForeignId,
                        boardTopic = $"football-match-{board.BoardEvent.ForeignId}", 
                        liveDataType = "boardDeactivated",
                        data = false
                    };
                    var deactivateReq = new HttpRequestMessage(HttpMethod.Post, $"{appConfig.JunaRestApiEndpoint}/boards/live");
                    deactivateReq.Headers.Add("Activation-Notifier-Api-Key", notifierKey);
                    deactivateReq.Content = new StringContent(JsonConvert.SerializeObject(liveData), Encoding.UTF8, "application/json");
                    HttpResponseMessage resp = await notifierClient.SendAsync(deactivateReq);
                }
            });
        }
    }
}
