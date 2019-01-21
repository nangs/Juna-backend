using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Web;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Xunit;
using Newtonsoft.Json;
using Juna.Feed.DomainModel;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Util;
using Juna.FeedFlows.DomainModel.Service;
using Juna.Feed.Service;
using Juna.Feed.Service.Interfaces;
using Juna.Feed.Service.Helpers;
using Juna.Feed.WebApi.Controllers;
using Juna.Feed.WebApi.Helpers;
using Juna.Feed.WebApi.Test.Databags;
using Moq;
using FluentAssertions;

namespace Juna.Feed.WebApi.Test
{
    public class ZoneControllerTest
    {
        private ZoneController _zoneController;

        public ZoneControllerTest()
        {
            var zoneServiceMock = new Mock<IZoneService>();
            zoneServiceMock.Setup(s => s.GetZones())
                .Returns(ZoneServiceTestData.CreateZonesOne());

            _zoneController = new ZoneController(zoneServiceMock.Object);
        }

        [Fact]
        public void GetZones_No_Parameters_Return_List_Of_Zones()
        {
            var actualResult = _zoneController.GetZones().Result as OkObjectResult;

            var expectedValue = Assert.IsType<List<Zone>>(actualResult.Value).Count;

            actualResult.Should().Equals(expectedValue);
        }
    }
}
