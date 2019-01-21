using System;
using System.Collections.Generic;
using System.Text;
using Juna.DDDCore.Common;
using Juna.Feed.DomainModel;
using Newtonsoft.Json;

namespace Juna.Feed.Dao
{
    public class ZoneDO : CosmosDBEntity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }
}
