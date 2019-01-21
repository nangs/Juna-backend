using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Juna.FeedFlows.Infrastructure.DTO;

namespace Juna.Newsfeed.Test.DTO
{
    public class FeedUpdateDTOUnitTest
    {
        [Theory]
        [InlineData("TestZoneNewsFeed.TestData.update.json")]
        // todo: This only tests inserts. Add tests for deleted items also
        public void Valid_FeedUpdateDTO_Succeeds(string jsonDataFilePath)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            var stream = thisAssembly.GetManifestResourceStream(jsonDataFilePath);
            var sample = new StreamReader(stream).ReadToEnd();
            JObject[] updates = JArray.Parse(sample).Select(j => (JObject)j).ToArray();
            var feedUpdateDTO = FeedUpdateDTO.FromJson(updates[0]);
            Assert.NotNull(feedUpdateDTO);
            Assert.NotEmpty(feedUpdateDTO.AppId);
            Assert.NotNull(feedUpdateDTO.DatePublished);
            Assert.NotNull(feedUpdateDTO.Feed);
            Assert.Single(feedUpdateDTO.ActivitiesAdded);
            Assert.Empty(feedUpdateDTO.ActivitiesDeleted);
            feedUpdateDTO.ActivitiesAdded.ToList().ForEach(a =>
            {
                Assert.NotEmpty(a.Id);
                Assert.NotEmpty(a.Verb);
                Assert.NotEmpty(a.Actor);
                Assert.NotEmpty(a.Time);
            }
            );
        }
    }
}
