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
using Juna.Feed.Service;
using Juna.Feed.Service.Helpers;
using Juna.FeedFlows.DomainModel.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights;
using Microsoft.WindowsAzure.Storage.Auth;
using Stream;

namespace Juna.Feed.Service.Test.Core
{
    public class DocumentDbFixture : IDisposable
    {
        private readonly IConfiguration configuration;

        public IContainer Container { get; private set; }

        public DocumentDbUtil DocumentDbUtil { get; private set; }

        public DocumentDbFixture()
        {
            var appConfig = new AppConfiguration();
            var path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            var configBuilder = new ConfigurationBuilder()
                        .SetBasePath(path)
                        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();

            configuration = configBuilder;

            ConfigurationBinder.Bind(configuration, appConfig);

            var azureMediaServicesClient = AzureMediaServiceClient
           .CreateMediaServicesClientAsync(appConfig.AzureMediaServices).Result;

            var builder = new ContainerBuilder();

            builder.Register<IAppConfiguration>(c => appConfig)
             .AsImplementedInterfaces()
             .AsSelf()
             .SingleInstance();

            builder.Register(c => new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }).As<JsonSerializer>();

            builder.Register<TelemetryClient>(c => new TelemetryClient());
            //builder.Register<IAppConfiguration>(c => this.appConfig);
            //builder.Register<IdentityHelper>(c => new IdentityHelper(c.Resolve<IAppConfiguration>()))
            //    .SingleInstance();

            #region database
            builder.Register<DocumentDbUtil>(c => new DocumentDbUtil(
                        endpointUrl: appConfig.AppSettings.CosmosdbEndpointUrl,
                        primaryKey: appConfig.AppSettings.CosmosdbPrimaryKey,
                        databaseName: appConfig.AppSettings.CosmosdbDatabaseName));
            #endregion database

            #region cloudApis
            builder.Register<StorageCredentials>(c => new StorageCredentials(
                appConfig.AppSettings.StorageKeyName,
                appConfig.AppSettings.StorageKeyValue
            )).SingleInstance();

            builder.Register<HttpClient>(c =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add(
                    appConfig.AppSettings.ThumbnailServiceApiKeyName,
                    appConfig.AppSettings.ThumbnailServiceApiKeyValue);
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return client;
            }).Named<HttpClient>("ThumbnailCognitiveApiClient").SingleInstance();

            builder.Register<StreamClient>(c =>
                    new StreamClient(appConfig.AppSettings.StreamAccessKey, appConfig.AppSettings.StreamSecret));

            builder.Register<HttpClient>(c =>
            {
                var client = new HttpClient { BaseAddress = new Uri(appConfig.AppSettings.FCMUrl) };
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={appConfig.AppSettings.FCMApplicationId}");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sender", $"id={appConfig.AppSettings.FCMSenderId}");
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return client;
            }).Named<HttpClient>("FCMClient").SingleInstance();

            // todo: Make this work with local storage
            builder.Register<BlobHelper>(c =>
                new BlobHelper(appConfig.AppSettings.ImageUploadStorageFolder)
            ).SingleInstance();

            #endregion cloudApis

            builder.Register<MapperConfiguration>(c => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<FeedItemProfile>();
                cfg.AddProfile<JunaUserProfile>();
                cfg.AddProfile<ActivityProfile>();
            })).SingleInstance().AutoActivate().AsSelf();

            builder.Register(c =>
            {
                var ctx = c.Resolve<IComponentContext>();
                var config = ctx.Resolve<MapperConfiguration>();
                return config.CreateMapper(t => ctx.Resolve(t));
            }).As<IMapper>();

            #region Repository
            builder.Register<FeedItemRepository>(c => new FeedItemRepository(
                documentDbUtil: c.Resolve<DocumentDbUtil>(),
                collectionName: appConfig.AppSettings.SocialDataCollectionName,
                mapper: c.Resolve<IMapper>()));

            builder.Register<JunaUserRepository>(c => new JunaUserRepository(
                documentDbUtil: c.Resolve<DocumentDbUtil>(),
                collectionName: appConfig.AppSettings.SocialDataCollectionName,
                mapper: c.Resolve<IMapper>()));

            builder.Register<ActivityRepository>(c => new ActivityRepository(
                documentDbUtil: c.Resolve<DocumentDbUtil>(),
                collectionName: appConfig.AppSettings.SocialDataCollectionName,
                mapper: c.Resolve<IMapper>()));

            builder.Register<CommentsRepository>(c => new CommentsRepository(
                documentDbUtil: c.Resolve<DocumentDbUtil>(),
                collectionName: appConfig.AppSettings.SocialDataCollectionName,
                mapper: c.Resolve<IMapper>()));

            builder.Register<BoardRepository>(c => new BoardRepository(
                documentDbUtil: c.Resolve<DocumentDbUtil>(),
                collectionName: appConfig.AppSettings.SocialDataCollectionName,
                mapper: c.Resolve<IMapper>()));

            #endregion Repository

            #region Service
            builder.Register<JunaUserService>(c => new JunaUserService(
                  c.Resolve<JunaUserRepository>(),
                  c.Resolve<Stream.StreamClient>(),
                  c.Resolve<ActivityRepository>()));

            builder.Register<FeedManagementService>(c => new FeedManagementService(
                c.Resolve<FeedItemRepository>(),
                c.Resolve<BoardRepository>(),
                c.Resolve<ActivityRepository>(),
                c.Resolve<JunaUserRepository>(),
                c.Resolve<ActivityManagementService>(),
                c.Resolve<FCMSenderService>(),
                c.Resolve<TelemetryClient>(),
                c.ResolveOptional<Stream.StreamClient>()
                ));

            builder.Register<ActivityManagementService>(c => new ActivityManagementService(
                c.Resolve<ActivityRepository>(),
                c.Resolve<BoardRepository>(),
                c.Resolve<FeedItemRepository>(),
                c.Resolve<JunaUserRepository>(),
                c.ResolveOptional<Stream.StreamClient>()));

            builder.Register<CommentsManagementService>(c => new CommentsManagementService(
                c.Resolve<CommentsRepository>(),
                c.Resolve<FeedItemRepository>(),
                c.ResolveOptional<Stream.StreamClient>()
                ));

            builder.Register<BoardManagementService>(c => new BoardManagementService(
                c.Resolve<BoardRepository>(),
                c.Resolve<ActivityRepository>(),
                c.Resolve<FeedItemRepository>(),
                c.Resolve<JunaUserRepository>(),
                c.ResolveOptional<Stream.StreamClient>()
                )
            );

            builder.Register<ModerationManagementService>(
                    c => new ModerationManagementService(
                        c.Resolve<ActivityRepository>(),
                        c.Resolve<JunaUserRepository>(),
                        c.Resolve<ActivityManagementService>(),
                        c.ResolveOptional<Stream.StreamClient>()
                        ));

            builder.Register<ContentUploadService>(c => new ContentUploadService(
                c.Resolve<FeedItemRepository>(),
                c.Resolve<ThumbnailService>(),
                c.Resolve<FCMSenderService>(),
                c.Resolve<StorageCredentials>(),
                c.Resolve<ActivityManagementService>(),
                c.Resolve<BoardRepository>(),
                c.Resolve<BlobHelper>(),
                c.ResolveOptional<Stream.StreamClient>()
            ));

            builder.Register<ThumbnailService>(c => new ThumbnailService(
                c.ResolveNamed<HttpClient>("ThumbnailCognitiveApiClient"),
                appConfig.AppSettings.ThumbnailServiceApiUrl,
                appConfig.AppSettings.ThumbnailWidth,
                appConfig.AppSettings.ThumbnailHeight,
                c.Resolve<StorageCredentials>(),
                c.Resolve<BlobHelper>(),
                appConfig.AzureMediaServices.ResourceGroup,
                appConfig.AzureMediaServices.AccountName,
                azureMediaServicesClient
                )
            );

            builder.Register<FCMSenderService>(c => new FCMSenderService(
                c.ResolveNamed<HttpClient>("FCMClient"),
                c.Resolve<TelemetryClient>()
                ));

            #endregion Service

            Container = builder.Build();

        }

        public void Dispose()
        {
            Container.Dispose();
        }
    }
}
