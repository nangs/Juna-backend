using System;
using Xunit;
using FluentAssertions;
using Juna.Zone.Feed.DomainModel;

namespace TestZoneNewsFeed.DomainModel
{
	
	public class FeedItemUnitTest
	{
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("dummy")]
		public void Cannot_Create_FeedItem_With_Invalid_Data(string type)
		{
			Action action = () =>
			{
				var feedItem = new FeedItem(type);
			};

			action.Should().Throw<InvalidOperationException>();
		}
	}
}
