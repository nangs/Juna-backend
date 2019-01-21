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
using Juna.Feed.Service.Helpers;
using Juna.Feed.WebApi.Controllers;
using Juna.Feed.WebApi.Helpers;
using Moq;
using FluentAssertions;

namespace Juna.Feed.WebApi.Test
{
    public class JunaUserControllerTest
    {
        private JunaUserController junaUserController;

        public JunaUserControllerTest()
        {
            var activityRepositoryMock = new Mock<ActivityRepository>();
            var junaUserServiceMock = new Mock<JunaUserService>();
            var telemetryClientMock = new Mock<TelemetryClient>();
            var junaUserRepositoryMock = new Mock<JunaUserRepository>();
            var identityHelperMock = new Mock<IdentityHelper>();

            junaUserController = new JunaUserController(activityRepositoryMock.Object, junaUserServiceMock.Object,
                telemetryClientMock.Object, junaUserRepositoryMock.Object, identityHelperMock.Object );
        }
    }
}
