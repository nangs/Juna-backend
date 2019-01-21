using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.Dao
{
   public class ActivityDO : CosmosDBEntity
   {
        [JsonProperty("actor")]
        public string Actor { get; set; }
        [JsonProperty("verb")]
        public string Verb { get; set; }
        [JsonProperty("object")]
        public string Object { get; set; }
        [JsonProperty("target")]
        public string Target { get; set; }
		// todo: Convert this field to DateTime
        [JsonProperty("time")]
        public string Time { get; set; }
    }
}
