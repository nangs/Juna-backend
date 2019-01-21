using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace Juna.Feed.Dao
{
    public class CosmosDBEntity : Document
    {
        // todo: I think I mapped Id to the in-built "id" property set on all Cosmosdb documents.
        // But I need to test it - Praneeth
        [JsonProperty("id")]
        public override string Id { get; set; }
		// This property determines what entity it gets converted to when retrieving from the global collection
		[JsonProperty("type")]
		public virtual string Type { get; set; }
	}
}
