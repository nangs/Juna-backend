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

namespace Juna.Feed.WebApi.Test.Databags
{
    public class ZoneServiceTestData
    {
        private static List<Zone> zonesOne = null;

        public static List<Zone> CreateZonesOne()
        {
            if (zonesOne == null)
            {
                zonesOne = new List<Zone>()
                {
                    new Zone
                    {
                        Id =Guid.Parse("696e5045-0833-42cc-9c48-500b40ea0240"),
                        Name = "Football",
                        Category = "FootballZone"
                    },
                    new Zone
                    {
                        Id =Guid.Parse("8d3c307f-c64e-4cdc-8024-355973df3f6d"),
                        Name = "Celebrity",
                        Category = "CelebrityZone"
                    }
                };
            }

            return zonesOne;
        }
    }
}
