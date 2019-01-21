using Juna.Feed.DomainModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Juna.Feed.Dao
{
	public class FeedItemDO : CosmosDBEntity
	{
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("actor")]
        public JunaUser Actor { get; set; }
        [JsonProperty("datePublished")]
        public DateTime DatePublished { get; set; }
        [JsonProperty("summary")]
        public string Summary { get; set; }
        [JsonProperty("thumbnail")]
        public Image Thumbnail { get; set; }
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; }
        [JsonProperty("contentType")]
        public string ContentType { get; set; }
        [JsonProperty("interactions")]
        public InteractionMetadata Interactions { get; set; }
    }
}