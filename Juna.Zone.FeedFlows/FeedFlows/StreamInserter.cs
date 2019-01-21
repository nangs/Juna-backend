using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Juna.Feed.DomainModel;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Stream;

namespace Juna.FeedFlows.FeedFlows
{
	public static class StreamInserter
    {
        [FunctionName("StreamInserter")]
        public static void Run(
			[CosmosDBTrigger("db-local", "socialData", ConnectionStringSetting="CosmosDBConnectionString", LeaseDatabaseName ="db-local", LeaseCollectionName = "leases", LeasesCollectionThroughput = 400)]
			IReadOnlyList<Document> changedFeedItems, 
			TraceWriter log,
			ExecutionContext context)
        {
			log.Info($"Received {changedFeedItems.Count} Documents from change feed processor");
			var config = new ConfigurationBuilder()
							.SetBasePath(context.FunctionAppDirectory)
							.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
							.AddEnvironmentVariables()
							.Build();

			// todo: parameterize these or add these to a constants file
			var streamAccessKey = config.GetSection("Values")["StreamAccessKey"];
			var streamSecret = config.GetSection("Values")["StreamSecret"];
			var feedItem = new FeedItem(FeedItem.NewsFeedItem);

			// todo: Parameterize this
			var feedGroup = "club_tournaments";
			var eplUser = "epl";
			log.Info($"Adding Feed Item with Title [{feedItem.Title}] to Stream feed [{eplUser}]");

			var streamClient = new StreamClient(streamAccessKey, streamSecret);

			try
			{
				log.Verbose(JsonConvert.SerializeObject(feedItem));

				// todo: read epl from a config file
				var eplFeed = streamClient.Feed(feedGroup, eplUser);
				var activity = new Stream.Activity(actor: eplUser, verb: "post", @object: feedItem.Id.ToString())
				{
					ForeignId = feedItem.Id.ToString(),
					// todo: This works in the short term by allowing me to populate old articles
					// at the appropriate time in the feed, but what about production?
					Time = feedItem.DatePublished
				};
				eplFeed.AddActivity(activity);
			}
			catch (Exception e)
			{
				log.Error($"threw exception {e.Message} while creating a user for {feedGroup}");
			}
		}
	}
}
