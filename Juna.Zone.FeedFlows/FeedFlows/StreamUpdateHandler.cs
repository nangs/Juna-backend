using System;
using Juna.FeedFlows.Infrastructure.DTO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Juna.FeedFlows.FeedFlows
{
    public static class StreamUpdateHandler
    {
        [FunctionName("StreamUpdateHandler")]
        public static void Run([QueueTrigger("stream-updates-queue", Connection = "AzureWebJobsStorage")]FeedUpdateDTO feedUpdate, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {feedUpdate}");
        }
    }
}
