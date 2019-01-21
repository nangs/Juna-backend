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
    public class ModerationManagementServiceTest : IClassFixture<DocumentDbFixture>
    {
        public ModerationManagementServiceTest(DocumentDbFixture fixture)
        {

        }

        [Fact]
        public void BlockUser_ValidInput_JunaUser_Blocked()
        {

        }

        [Fact]
        public void UnBlockUser_ValidInput_JunaUser_UnBlocked()
        {

        }

        [Fact]
        public void BanUser_ValidInput_JunaUser_Banned()
        {

        }

        [Fact]
        public void UnBanUser_ValidInput_JunaUser_UnBanned()
        {

        }

        [Fact]
        public void MuteUser_ValidInput_JunaUser_Muted()
        {

        }

        [Fact]
        public void UnMuteUser_ValidInput_JunaUser_UnMuted()
        {

        }

        [Fact]
        public void Report_ValidInput_Activity_Report_Returned()
        {

        }
    }
}
