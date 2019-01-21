using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Moq;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using AutoMapper;
using Juna.Feed.Repository.Mapping;
using Juna.Feed.Repository.Util;
using Juna.Feed.Repository;
using Juna.Feed.DomainModel;
using Juna.Feed.Service;
using Juna.Feed.Service.Helpers;
using Juna.FeedFlows.DomainModel.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Juna.Feed.Service.Test.Databags
{
    public class ActivityManagementServiceData
    {
        #region Activity
        private static Activity ActivityOne = null;
        private static Activity ActivityTwo = null;
        private static Activity ActivityThree = null;

        public static Activity CreateActivityOne()
        {
            if (ActivityOne == null)
            {
                ActivityOne = new Activity()
                {
                    Actor = "Actor One",
                    ForeignId = "d40cf8c4-7606-4bc6-a64d-0141396c5fc5",
                    Object = "Object One",
                    Target = "Target One",
                    Time = DateTime.Now.AddHours(1).ToString(),
                    Verb = "Verb One"
                };
            }

            return ActivityOne;
        }

        public static Activity CreateActivityTwo()
        {
            if (ActivityTwo == null)
            {
                ActivityTwo = new Activity()
                {
                    Actor = "Actor two",
                    ForeignId = "15b97887-414e-4daf-9615-c7b5163be8e9",
                    Object = "Object two",
                    Target = "Target two",
                    Time = DateTime.Now.AddHours(2).ToString(),
                    Verb = "Verb two"
                };
            }

            return ActivityTwo;
        }

        public static Activity CreateActivityThree()
        {
            if (ActivityThree == null)
            {
                ActivityThree = new Activity()
                {
                    Actor = "Actor three",
                    ForeignId = "3138094d-a7f7-4806-894f-445d5032b990",
                    Object = "Object three",
                    Target = "Target three",
                    Time = DateTime.Now.AddHours(3).ToString(),
                    Verb = "Verb three"
                };
            }

            return ActivityThree;
        }

        #endregion

        #region FeedItem

        private static string TypeOne;
        private static string TypeTwo;
        private static string TypeThree;

        private static FeedItem FeedItemOne = null;
        private static FeedItem FeedItemTwo = null;
        private static FeedItem FeedItemThree = null;

        public static FeedItem CreateFeedItemOne()
        {
            if (FeedItemOne == null)
            {
                FeedItemOne = new FeedItem(TypeOne)
                {

                };
            }

            return FeedItemOne;
        }

        public static FeedItem CreateFeedItemTwo()
        {
            if (FeedItemTwo == null)
            {
                FeedItemTwo = new FeedItem(TypeTwo)
                {

                };
            }

            return FeedItemTwo;
        }

        public static FeedItem CreateFeedItemThree()
        {
            if (FeedItemThree == null)
            {
                FeedItemThree = new FeedItem(TypeThree)
                {

                };
            }

            return FeedItemThree;
        }

        #endregion

        #region JunaUser

        private static JunaUser JunaUserOne = null;
        private static JunaUser JunaUserTwo = null;
        private static JunaUser JunaUserThree = null;

        public static JunaUser CreateJunaUserOne()
        {
            if (JunaUserOne == null)
            {
                JunaUserOne = new JunaUser
                {
                    Id = new Guid("a9f07571-b42d-44ad-a6f4-f7baa766cf31"),
                    ObjectId = new Guid("9ffaeca6-ae17-4d76-bfe1-cc00b0a56fd6").ToString(),
                    City = "Tangquan",
                    Country = "China",
                    DisplayName = "Valeria Cossor",
                    EmailAddress = "rgeroldini0@privacy.gov.au",
                    GivenName = "Valeria",
                    IdentityProvider = "",
                    JobTitle = "Budget/Accounting Analyst IV",
                    PostalCode = "12345",
                    StreetAddress = "283 Caliangt Way",
                    Surname = "Cossor"
                };
            }

            return JunaUserOne;
        }

        public static JunaUser CreateJunaUserTwo()
        {
            if (JunaUserTwo == null)
            {
                JunaUserTwo = new JunaUser
                {
                    Id = new Guid("cc2a8fb5-bccd-4550-851a-67cdd0cc8adc"),
                    ObjectId = new Guid("c595a4bb-ba61-468a-b7fd-2210cf960eed").ToString(),
                    City = "Bandung",
                    Country = "Indonesia",
                    DisplayName = "Valeria Cossor",
                    EmailAddress = "vcossor1@usatoday.com",
                    GivenName = "Valeria",
                    IdentityProvider = "",
                    JobTitle = "Nurse Practicioner",
                    PostalCode = "50000",
                    StreetAddress = "9249 Summerview Court",
                    Surname = "Cossor"
                };
            }

            return JunaUserTwo;
        }

        public static JunaUser CreateJunaUserThree()
        {
            if (JunaUserThree == null)
            {
                JunaUserThree = new JunaUser
                {
                    Id = new Guid("a9532d4d-7308-436f-b647-2ee8e167507f"),
                    ObjectId =  new Guid("266889b7-1522-413d-bcd1-fd4e5b5abec2").ToString(),
                    City = "Buenos Aires",
                    Country = "Argentina",
                    DisplayName = "Gabi Harback",
                    EmailAddress = "gharback4@ed.gov",
                    GivenName = "Gabi",
                    IdentityProvider = "",
                    JobTitle = "Physical Therapy Assistant",
                    PostalCode = "3334",
                    StreetAddress = "0602 Aberg Street",
                    Surname = "Harback"
                };
            }

            return JunaUserThree;
        }

        #endregion

    }
}
