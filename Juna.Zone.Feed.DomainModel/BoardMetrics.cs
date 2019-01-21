using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.DomainModel
{
    public class BoardMetrics
    {

        [JsonProperty("noOfParticipants")]
        public int NoOfParticipants { get; set; }
        [JsonProperty("noOfInteractions")]
        public int NoOfInteractions { get; set; }
        [JsonProperty("noOfFeedItems")]
        public int NoOfFeedItems { get; set; }
        [JsonProperty("userMetrics")]
        public UserMetrics UserMetrics { get; set; }
    }
}
