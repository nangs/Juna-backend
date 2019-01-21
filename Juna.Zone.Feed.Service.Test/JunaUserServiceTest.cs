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
    public class JunaUserServiceTest : IClassFixture<DocumentDbFixture>
    {
        public JunaUserServiceTest(DocumentDbFixture fixture)
        {

        }

        [Fact]
        public void GetJunaUserByObjectId_ValidInput_By_ObjectId_JunaUser_Returned()
        {

        }

        [Fact]
        public void Create_JunaUser_As_Input_JunaUser_Returned()
        {

        }

        [Fact]
        public void CreateNewOrGetExistingUser_ValidInput_JunaUser_Created()
        {

        }

        [Fact]
        public void GetJunaUserByEmail_ValidInput_JunaUser_Returned()
        {

        }

        [Fact]
        public void FollowUser_ValidInput_JunaUser_Followed()
        {

        }

        [Fact]
        public void UnFollowUser_ValidInput_JunaUser_UnFollowed()
        {

        }
    }
}
