using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Juna.FeedFlows.Infrastructure.DTO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Stream;

namespace Juna.FeedFlows.Controller
{
    public static class ActivityWebHook
    {
        [FunctionName("ActivityWebHook")]
        public static HttpResponseMessage Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "activities")]
			HttpRequestMessage req,
			[Queue("unprocessed-feed-items", Connection = "AzureWebJobsStorage")]ICollector<ActivityDTO> updateQueue,
			TraceWriter log)
        {
            log.Info("Activity Controller invoked by Streams webhook.");
			if (req.Method == HttpMethod.Get) {
				// stream api requires that the webhook answer back with the stream api key
				return req.CreateResponse(HttpStatusCode.OK, "pgm68aj7ec9e");
			}

			try
			{
				var jsonStream = req.Content.ReadAsStreamAsync();
				var serializer = new JsonSerializer();

				using (var streamReader = new StreamReader(jsonStream.Result))
				{
					// todo: fix this ugliness. There's got to be a better way to do this.
					var dataString = streamReader.ReadToEnd();
					log.Info(dataString);
					// todo: serialize the updates
					var data = FeedUpdateDTO.FromJson(dataString);
					return req.CreateResponse(HttpStatusCode.OK);
				}
			}
			catch (Exception e)
			{
				log.Error(e.StackTrace);
				return req.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
			}
		}
	}
}
