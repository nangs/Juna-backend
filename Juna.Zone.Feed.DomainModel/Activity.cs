using Juna.DDDCore.Common;
using Newtonsoft.Json;

namespace Juna.Feed.DomainModel
{
    public class Activity: AggregateRoot
    {
        // todo: Now there are 3 classes that handle Activity. We need to reduce it to two
        // todo: actor should be a composite of type [actorType:Guid]
        [JsonProperty("actor")]
        public string Actor { get; set; }
        [JsonProperty("verb")]
        public string Verb { get; set; }
        // todo: object should be a composite of type [objectType:Guid]
        [JsonProperty("object")]
        public string Object { get; set; }
        // todo: target should be a composite of structure [targetType:Guid]
        [JsonProperty("target")]
        public string Target { get; set; }
        // todo: foreignId should be a composite of structure [entityType:Guid]
        [JsonProperty("foreignId")]
        public string ForeignId { get; set; }
        [JsonProperty("time")]
        public string Time { get; set; }
    }
}
