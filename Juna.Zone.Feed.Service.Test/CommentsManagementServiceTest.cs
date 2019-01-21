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
    public class CommentsManagementServiceTest : IClassFixture<DocumentDbFixture>
    {
        public CommentsManagementServiceTest(DocumentDbFixture fixture)
        {

        }

        [Fact]
        public void GetCommentsByFeedItem_ValidInput_Return_Comment_By_FeedItemId()
        {

        }

        [Fact]
        public void StoreComments_ValidInput_Return_Comment()
        {

        }

        [Fact]
        public void ReplyToComment_ValidInput_Return_Comment()
        {

        }

        [Fact]
        public void DeleteComments_ValidInput_Comment_Deleted()
        {

        }

    }
}
