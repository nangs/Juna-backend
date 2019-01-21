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
    public class FeedManagementServiceTest : IClassFixture<DocumentDbFixture>
    {
        public FeedManagementServiceTest(DocumentDbFixture fixture)
        {

        }

        [Fact]
        public void StoreItemWithUniqueUrl_ValidInput_FeedItem_Stored()
        {

        }

        [Fact]
        public void StoreItemWithUniqueUrlAsync_ValidInput_FeedItem_Stored()
        {

        }

        [Fact]
        public void GetFeedItems_ValidInput_List_Of_FeedItem_Returned()
        {

        }

        [Fact]
        public void CreateFeedItem_ValidInput_FeedItem_Created()
        {

        }

        [Fact]
        public void DeleteFeedItem_ValidInput_FeedItem_Deleted()
        {

        }

        [Fact]
        public void CreateFeedItem_ValidInput_Of_List_FeedItem_FeedItem_Created()
        {

        }
    }
}
