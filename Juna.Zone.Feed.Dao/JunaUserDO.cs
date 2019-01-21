using Newtonsoft.Json;

namespace Juna.Feed.Dao
{
    public class JunaUserDO: CosmosDBEntity
    {
		[JsonProperty("objectId")]
		public string ObjectId { get; set; }
		[JsonProperty("displayName")]
		public string DisplayName { get; set; }
		[JsonProperty("emailAddress")]
		public string EmailAddress { get; set; }
		[JsonProperty("country")]
		public string Country { get; set; }
		[JsonProperty("city")]
		public string City { get; set; }
		[JsonProperty("identityProvider")]
		public string IdentityProvider { get; set; }
		[JsonProperty("jobTitle")]
		public string JobTitle { get; set; }
		[JsonProperty("postalCode")]
		public string PostalCode { get; set; }
		[JsonProperty("streetAddress")]
		public string StreetAddress { get; set; }
		[JsonProperty("givenName")]
		public string GivenName { get; set; }
		[JsonProperty("surname")]
		public string Surname { get; set; }
	}
}
