using Juna.DDDCore.Common;
using Newtonsoft.Json;
using System;

namespace Juna.Feed.DomainModel
{
    public class JunaUser : AggregateRoot
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

        public UserPreference UserPreference { get; set; }

        public JunaUser() { }

        public JunaUser(string _objectId, string _displayName, string _emailAddress, string _identityProvider, string _country) 
        {
            if (string.IsNullOrWhiteSpace(_objectId)) throw new InvalidOperationException("Object Id should not be empty");
            if (string.IsNullOrWhiteSpace(_displayName)) throw new InvalidOperationException("DisplayName should not be empty");
            if (string.IsNullOrWhiteSpace(_emailAddress)) throw new InvalidOperationException("EmailAddress should not be empty");
            if (string.IsNullOrWhiteSpace(_identityProvider)) throw new InvalidOperationException("Identity Provider should not be empty");
            if (string.IsNullOrWhiteSpace(_country)) throw new InvalidOperationException("Country name should not be empty");

            ObjectId = _objectId;
            DisplayName = _displayName;
            EmailAddress = _emailAddress;
            IdentityProvider = _identityProvider;
            Country = _country;
        }

        public static Boolean IsValidJunaUser(JunaUser user)
        {
            // The five main fields are already validated during serialization when Newtonsoft 
            // serializer tries to create a JunaUser object
            var isValid = true;

            isValid &= IsValidEmail(user.EmailAddress);
            isValid &= IsValidGuid(user.ObjectId);
            // todo: validate country
            // todo: validate identity provider
            return isValid;
        }

        // todo: Move this to a utility method
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // todo: Move this to a utility class
        private static bool IsValidGuid(string guidString)
        {
            try
            {
                var guid = new Guid(guidString);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}