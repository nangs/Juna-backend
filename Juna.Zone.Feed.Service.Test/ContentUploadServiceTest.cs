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
    public class ContentUploadServiceTest : IClassFixture<DocumentDbFixture>
    {
        public ContentUploadServiceTest(DocumentDbFixture fixture)
        {

        }

        [Fact]
        public void UploadFile_ValidInput_File_Uploaded()
        {

        }

        [Fact]
        public void UploadAndSaveFeedItemAsync_ValidInput_SaveFeedItem()
        {

        }
    }
}
