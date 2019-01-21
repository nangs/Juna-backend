using FluentAssertions;
using System;
using Xunit;
using Juna.Zone.Feed.DomainModel;

namespace TestZoneNewsFeed.DomainModel
{
	public class JunaUserUnitTest
    {
        [Theory]
        [InlineData(null,                                   "John Smith", "john@smith.com", "google", "USA")]
        [InlineData("",                                     "John Smith", "john@smith.com", "google", "USA")]
        [InlineData("    ",                                 "John Smith", "john@smith.com", "google", "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", null,         "john@smith.com", "google", "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "",           "john@smith.com", "google", "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "  ",         "john@smith.com", "google", "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "John Smith", null,             "google", "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "John Smith", "",               "google", "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "John Smith", "   ",            "google", "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "John Smith", "john@smith.com", null,     "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "John Smith", "john@smith.com", "",       "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "John Smith", "john@smith.com", "  ",     "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "John Smith", "john@smith.com", "google", null)]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "John Smith", "john@smith.com", "google", "")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "John Smith", "john@smith.com", "google", "   ")]
        void Cannot_Create_JunaUser_With_Invalid_Data(string _objectId, string _displayName, string _emailAddress, string _identityProvider, string _country)
        {
            Action action = () =>
            {
                var feed = new JunaUser(_objectId, _displayName, _emailAddress, _identityProvider, _country);
            };
            action.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineData("garbage guid", "John Smith", "john@smith.com", "facebook", "USA")]
        [InlineData("36104174-ba2f-46e3-a190-6ab56ff64a1b", "John Smith", "garbage email", "facebook", "USA")]
        void IsValidJunaUser_Returns_False_For_Invalid_Data(string _objectId, string _displayName, string _emailAddress, string _identityProvider, string _country)
        {
            Assert.False(JunaUser.IsValidJunaUser(new JunaUser(_objectId, _displayName, _emailAddress, _identityProvider, _country)));
        }
    }
}
