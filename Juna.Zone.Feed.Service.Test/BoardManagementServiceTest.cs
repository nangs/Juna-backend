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
    public class BoardManagementServiceTest : IClassFixture<DocumentDbFixture>
    {
        private BoardManagementService pBoardManagementService;

        public BoardManagementServiceTest(DocumentDbFixture fixture)
        {
            pBoardManagementService = fixture.Container.Resolve<BoardManagementService>();
        }

        [Fact]
        public void CreateBoard_ValidInput_Board_Created()
        {

        }

        [Fact]
        public void CreatePrivateBoard_ValidInput_Private_Board_Created()
        {

        }

        [Fact]
        public void StoreBoardWithUniqueId_ValidInput_Board_With_UniqueId_Created()
        {

        }

        [Fact]
        public void UserEntersBoard_ValidInput_User_Enter_Board_Created()
        {

        }

        [Fact]
        public void UserExitBoard_ValidInput_User_Exit_Board_Created()
        {

        }

        [Fact]
        public void UserFollowsBoard_ValidInput_User_Follow_Board()
        {

        }

        [Fact]
        public void UserUnFollowsBoard_ValidInput_User_UnFollow_Board()
        {

        }

        [Fact]
        public void GetBoard_ValidInput_Return_Board()
        {

        }

        [Fact]
        public void GetByDate_ValidInput_Return_List_Of_Board_By_Date()
        {

        }

        [Fact]
        public void GetByBoardEvent_ValidInput_Return_Board_By_Event()
        {

        }
    }
}
