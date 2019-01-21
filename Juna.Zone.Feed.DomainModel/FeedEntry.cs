using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.DomainModel
{
    public class FeedEntry 
    {
        [JsonProperty("activity")]
        public Activity Activity { get; set; }
        [JsonProperty("feedItem")]
        public FeedItem FeedItem { get; set; }
    }
}
