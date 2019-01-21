using Juna.DDDCore.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.DomainModel
{
    public class LiveEvent
    {
        [JsonProperty("liveEventType")]
        public string LiveEventType { get; set; }

        [JsonProperty("foreignId")]
        public string ForeignId { get; set; }

        [JsonProperty("boardTopic")]
        public string BoardTopic { get; set; }

        [JsonProperty("liveDataType")]
        public string LiveDataType { get; set; }

        [JsonProperty("scoreData")]
        public dynamic ScoreData { get; set; }
        [JsonProperty("matchEventList")]
        public dynamic MatchEventList { get; set; }
        [JsonProperty("commentaryList")]
        public dynamic CommentaryList { get; set; }
        [JsonProperty("liveTimeStatus")]
        public dynamic LiveTimeStatus { get; set; }
       
    }
}
