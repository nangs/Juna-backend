using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Autofac;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Mapping;
using Juna.Feed.Repository.Util;
using Juna.Feed.Service;
using Juna.Feed.Service.Helpers;
using Juna.Feed.Service.Interfaces;
using Juna.FeedFlows.DomainModel.Service;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.AspNetCore.Hosting;
using Stream;
using Microsoft.Azure.Management.Media;

namespace Juna.Feed.WebApi.Helpers
{
	public class CustomBootstrapper : Module
    {
        private IAppConfiguration appConfig;
        private string AzureThumbnailConfig;
        private readonly IHostingEnvironment environment;

        public CustomBootstrapper(IAppConfiguration appConfig, IHostingEnvironment env)
        {
            this.appConfig = appConfig;
            this.environment = env;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var configBuilder = new ConfigurationBuilder()
                          .SetBasePath(this.environment.ContentRootPath)
                          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables()
                          .Build();

            var azureMediaServicesClient = AzureMediaServiceClient
				.CreateMediaServicesClientAsync(appConfig.AzureMediaServices).Result;

            builder.Register<IAppConfiguration>(c => appConfig)
				.AsImplementedInterfaces()
				.AsSelf()
				.SingleInstance();

            AzureThumbnailConfig = File.ReadAllText(
				Path.Combine(this.environment.ContentRootPath, @"ThumbnailPreset_JSON.json"));


            builder.Register(c => new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }).As<JsonSerializer>().AsSelf().AsImplementedInterfaces();

            builder.Register<TelemetryClient>(c => new TelemetryClient());
            builder.Register<IAppConfiguration>(c => this.appConfig);
			builder.Register<IdentityHelper>(c => new IdentityHelper(c.Resolve<IAppConfiguration>()))
				.SingleInstance();

            #region database
            builder.Register<DocumentDbUtil>(c => new DocumentDbUtil(
                        endpointUrl: appConfig.AppSettings.CosmosdbEndpointUrl,
                        primaryKey: appConfig.AppSettings.CosmosdbPrimaryKey,
                        databaseName: appConfig.AppSettings.CosmosdbDatabaseName)).SingleInstance().AsSelf();
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
                mapper: c.Resolve<IMapper>())).AsSelf();
            builder.Register<JunaUserRepository>(c => new JunaUserRepository(
                documentDbUtil: c.Resolve<DocumentDbUtil>(),
                collectionName: appConfig.AppSettings.SocialDataCollectionName,
                mapper: c.Resolve<IMapper>())).AsSelf();
            builder.Register<ActivityRepository>(c => new ActivityRepository(
                documentDbUtil: c.Resolve<DocumentDbUtil>(),
                collectionName: appConfig.AppSettings.SocialDataCollectionName,
                mapper: c.Resolve<IMapper>())).AsSelf();
            builder.Register<CommentsRepository>(c => new CommentsRepository(
                documentDbUtil: c.Resolve<DocumentDbUtil>(),
                collectionName: appConfig.AppSettings.SocialDataCollectionName,
                mapper: c.Resolve<IMapper>())).AsSelf();
            builder.Register<BoardRepository>(c => new BoardRepository(
                documentDbUtil: c.Resolve<DocumentDbUtil>(),
                collectionName: appConfig.AppSettings.SocialDataCollectionName,
                mapper: c.Resolve<IMapper>())).AsSelf();
            builder.Register<ZoneRepository>(c => new ZoneRepository(
             documentDbUtil: c.Resolve<DocumentDbUtil>(),
             collectionName: appConfig.AppSettings.SocialDataCollectionName,
             mapper: c.Resolve<IMapper>())).AsSelf();
            #endregion Repository

            #region Service
            builder.Register<JunaUserService>(c => new JunaUserService(
                  c.Resolve<JunaUserRepository>(),
                  c.Resolve<Stream.StreamClient>(),
                  c.Resolve<ActivityRepository>())).AsSelf();

            builder.Register<FeedManagementService>(c => new FeedManagementService(
                c.Resolve<FeedItemRepository>(),
                c.Resolve<BoardRepository>(),
                c.Resolve<ActivityRepository>(),
                c.Resolve<JunaUserRepository>(),
                c.Resolve<ActivityManagementService>(),
                c.Resolve<FCMSenderService>(),
                c.Resolve<TelemetryClient>(),
                c.ResolveOptional<Stream.StreamClient>()
                )).AsSelf();

            builder.Register<ActivityManagementService>(c => new ActivityManagementService(
                c.Resolve<ActivityRepository>(),
                c.Resolve<BoardRepository>(),
                c.Resolve<FeedItemRepository>(),
                c.Resolve<JunaUserRepository>(),
                c.ResolveOptional<Stream.StreamClient>())).AsSelf();

            builder.Register<CommentsManagementService>(c => new CommentsManagementService(
                c.Resolve<CommentsRepository>(),
                c.Resolve<FeedItemRepository>(),
                c.ResolveOptional<Stream.StreamClient>()
                )).AsSelf();

            builder.Register<BoardManagementService>(c => new BoardManagementService(
                c.Resolve<BoardRepository>(),
                c.Resolve<ActivityRepository>(),
                c.Resolve<FeedItemRepository>(),
                c.Resolve<JunaUserRepository>(),
                c.ResolveOptional<Stream.StreamClient>()
                )
            ).AsSelf();

            builder.Register(c => new ZoneService(c.Resolve<ZoneRepository>())).As<IZoneService>();

            builder.Register<ModerationManagementService>(
                    c => new ModerationManagementService(
                        c.Resolve<ActivityRepository>(),
                        c.Resolve<JunaUserRepository>(),
                        c.Resolve<ActivityManagementService>(),
                        c.ResolveOptional<Stream.StreamClient>()
                        )).AsSelf();

            builder.Register<ContentUploadService>(c => new ContentUploadService(
                c.Resolve<FeedItemRepository>(),
                c.Resolve<ThumbnailService>(),
                c.Resolve<FCMSenderService>(),
                c.Resolve<StorageCredentials>(),
                c.Resolve<ActivityManagementService>(),
                c.Resolve<BoardRepository>(),
                c.Resolve<BlobHelper>(),
                c.ResolveOptional<Stream.StreamClient>()
            )).AsSelf();
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
            ).AsSelf();
            builder.Register<FCMSenderService>(c => new FCMSenderService(
                c.ResolveNamed<HttpClient>("FCMClient"),
                c.Resolve<TelemetryClient>()
                ));
            #endregion Service
        }
    }
}
