using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Juna.Feed.DomainModel;
using Juna.FeedFlows.DomainModel;

namespace Juna.Newsfeed.Test.DomainModel
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
