using System;
using System.Collections.Generic;
using System.Text;
using Juna.DDDCore.Common;
using Newtonsoft.Json;

namespace Juna.Feed.DomainModel
{
    public class Zone : AggregateRoot
    {
        [JsonProperty("id")]
        public override Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }
}
