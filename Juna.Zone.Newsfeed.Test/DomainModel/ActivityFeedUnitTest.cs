using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Juna.Feed.DomainModel;
using Juna.FeedFlows.DomainModel;

namespace Juna.Newsfeed.Test.DomainModel
{
    public class ActivityFeedUnitTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a:b:c")]
        public void Cannot_Create_ActivityFeed_With_Invalid_Data(string feedString)
        {
            Action action = () =>
            {
                var feed = new ActivityFeed(feedString);
            };
            action.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineData("myfeed")]
        public void Valid_ActivityFeed_Has_Name(string feedString)
        {
            var feed = new ActivityFeed(feedString);
            Assert.Equal(feedString, feed.Name);
        }

        // todo: This test belongs in the infrastructure layer because we need to be translating
        // the feed name to an ActivityFeed object via a database call
        [Theory]
        [InlineData("epl:myfeed")]
        public void Valid_ActivityFeed_Has_Name_And_Type(string feedString)
        {
            var feed = new ActivityFeed(feedString);
            Assert.StartsWith(feed.Group.Name, feedString);
            Assert.EndsWith(feed.Name, feedString);
        }
    }
}
