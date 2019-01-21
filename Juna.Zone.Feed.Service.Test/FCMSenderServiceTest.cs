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
    public class FCMSenderServiceTest : IClassFixture<DocumentDbFixture>
    {
        public FCMSenderServiceTest(DocumentDbFixture fixture)
        {

        }

        [Fact]
        public void SendFcmBoardNotification_ValidInput_Notification_Sent()
        {

        }

        [Fact]
        public void SendFcmUserNotification_ValidInput_Notification_Sent()
        {

        }

        [Fact]
        public void SendFcmNotificationToTopic_ValidInput_Notification_Sent()
        {

        }

        [Fact]
        public void SendBoardInviteNotification_ValidInput_Notification_Sent()
        {

        }

        [Fact]
        public void SendBoardLiveData_ValidInput_BoardLiveData_Sent()
        {

        }
    }
}
