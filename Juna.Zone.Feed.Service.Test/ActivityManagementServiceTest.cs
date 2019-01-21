using System;
using Xunit;
using Moq;
using Juna.Feed.Dao;
using Juna.Feed.DomainModel;
using Juna.Feed.Repository;
using Juna.Feed.Service;
using Juna.DDDCore;
using Microsoft.Azure.Documents;
using Juna.FeedFlows.DomainModel.Service;
using Juna.Feed.Service.Test.Core;
using Juna.Feed.Repository.Util;
using Autofac;

namespace Juna.Feed.Service.Test
{
    public class ActivityManagementServiceTest : IClassFixture<DocumentDbFixture>
    {
        private ActivityManagementService pActivityManagementService;

        public ActivityManagementServiceTest(DocumentDbFixture fixture)
        {
            //var activityRepo = fixture.Container.Resolve<ActivityRepository>();
            //var boardRepo = fixture.Container.Resolve<BoardRepository>();
            //var feedItemRepo = fixture.Container.Resolve<FeedItemRepository>();
            //var junaUserRepo = fixture.Container.Resolve<JunaUserRepository>();
            //var streamClient = fixture.Container.Resolve<Stream.StreamClient>();

            //pActivityManagementService = new ActivityManagementService(activityRepo, boardRepo, feedItemRepo, junaUserRepo, streamClient);
                
            pActivityManagementService = fixture.Container.Resolve<ActivityManagementService>();
        }

        [Fact]
        public void StoreUniqueActivity_ValidInput_ReturnActivityObject()
        {

        }

        [Fact]
        public async void StoreUniqueActivity_validInput_returnActivityObject()
        {
            var activityObj = new Activity
            {
                Id = Guid.NewGuid(),
            };

            var actualResult = await pActivityManagementService.StoreUniqueActivityAsync(activityObj);

            Assert.Equal(activityObj.Id, actualResult.Id);
        }

        [Fact]
        public void DeleteActivity_validInput_ActivityDeleted()
        {

        }

        [Fact]
        public void LikeFeedItem_ValidInputParameter_Should_Be_LikedFeedItem()
        {

        }

        [Fact]
        public void UnLike_ValidInputParameter_Should_Be_UnLiked()
        {

        }

        [Fact]
        public void DislikeFeedItem_ValidInputParameter_Should_Be_DislikeFeedItem()
        {

        }

        [Fact]
        public void UndoDisLike_ValidInputParameter_Should_Be_UndoDisLiked()
        {

        }

        [Fact]
        public void ShareFeedItem_ValidInputParameter_Should_Be_ShareFeedItem()
        {

        }

        [Fact]
        public void UnshareFeedItem_ValidInputParameter_Should_Be_UnshareFeedItem()
        {

        }

        [Fact]
        public void PinFeedItem_ValidInputParameter_Should_Be_PinFeedItem()
        {

        }

        [Fact]
        public void DeletePin_ValidInputParameter_Should_Be_DeletePin()
        {

        }
    }
}
