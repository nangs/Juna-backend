using Juna.DDDCore.Common;
using Juna.Feed.DomainModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.Dao
{
    public class BoardDO : CosmosDBEntity
    {
        [JsonProperty("displayname")]
        public string Displayname { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        // todo: MatchStratTime only applies to football match boards. 
        // This doesn't belong on the board, but in a child class
        [JsonProperty("startDate")]
        public string StartDate { get; set; }
        [JsonProperty("endDate")]
        public string EndDate { get; set; }
        [JsonProperty("topicName")]
        public string TopicName { get; set; }
        [JsonProperty("boardType")]
        public string BoardType { get; set; }


        [JsonProperty("owner")]
        public JunaUser Owner { get; set; }
        [JsonProperty("zone")]
        public string Zone { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("color")]
        public string Color { get; set; }
        [JsonProperty("boardIcon")]
        public Image BoardIcon { get; set; }
        [JsonProperty("creatTime")]
        public string CreateTime { get; set; }
        [JsonProperty("boardExpiry")]
        public string BoardExpiry { get; set; }

        [JsonProperty("isActive")]
        public Boolean IsActive { get; set; }
        [JsonProperty("createdBy")]
        public JunaUser CreatedBy { get; set; }
        [JsonProperty("boardMetrics")]
        public BoardMetrics BoardMetrics { get; set; }
        [JsonProperty("boardEvent")]
        public BoardEvent BoardEvent { get; set; }
        [JsonProperty("interactions")]
        public BoardInteractionMetadata Interactions { get; set; }

    }
}
