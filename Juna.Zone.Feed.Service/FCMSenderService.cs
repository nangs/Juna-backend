using Juna.Feed.DomainModel;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Juna.Feed.Service
{
    public class JunaNotification
    {
        public string Title { get; set; }
        public string ThumbnailImageUrl { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
        public string ImageUrl { get; set; }
        public string Action { get; set; }
        public string Actor { get; set; }
        public string ContentType { get; set; }
        public long ForeignId { get; set; }
    }

    public class BoardInviteNotification
    {
        public Guid BoardId { get; set; }
        public string UserId { get; set; }
        public string InviterName { get; set; }
        public string InviteeUserId { get; set; }
        public string InvitationLink { get; set; }
    }

    public class FCMSenderService
    {
        private static readonly JsonSerializerSettings defaultSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public const string CREATE_OPERATION = "create";
		public const string UPDATE_OPERATION = "update";
		public static readonly string[] OperationTypes = { CREATE_OPERATION, UPDATE_OPERATION};

		private TelemetryClient logger;
		private HttpClient fcmClient;

		public FCMSenderService(HttpClient client, TelemetryClient trace)
		{
			fcmClient = client;
			logger = trace;
		}

		public async Task SendFcmBoardNotification(JunaNotification junaNotification, Board board, string operationType)
		{
			await SendFcmNotificationToTopic(junaNotification, $"Board-{board.Id}", operationType);
		}

		public async Task SendFcmUserNotification(JunaNotification junaNotification, JunaUser user, string operationType)
		{
			await SendFcmNotificationToTopic(junaNotification, $"JunaUser-{user.ObjectId}", operationType);
		}

		private async Task SendFcmNotificationToTopic(JunaNotification junaNotification, string targetTopic, string operationType)
		{
            // todo: Validate that operationType sent is a valid operation type.
            // todo: validate junanotification
            var fcmData = new
			{
				to = $"/topics/{targetTopic}",
                data = junaNotification
            };
			var json = JsonConvert.SerializeObject(fcmData, new JsonSerializerSettings { 
				NullValueHandling = NullValueHandling.Ignore,
				DateFormatHandling = DateFormatHandling.IsoDateFormat,
				Formatting = Formatting.Indented,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			});
			var size = json.Length;
			var content = new StringContent(JsonConvert.SerializeObject(fcmData, defaultSerializerSettings), Encoding.UTF8, "application/json");
			
			var result = await fcmClient.PostAsync("", content);
			if (result.IsSuccessStatusCode)
			{
				logger.TrackTrace($"Successfully sent push notification");
			}
			else
			{
				logger.TrackTrace($"Received error code {result.StatusCode} when trying to send push notification");
			}
			logger.TrackTrace($"Received the following response body from FCM");
			logger.TrackTrace($"===============================================");
			logger.TrackTrace($"Response body { result.Content.ReadAsStringAsync().Result}");
			logger.TrackTrace($"===============================================");
		}

        public async Task SendBoardInviteNotification(BoardInviteNotification boardInviteNotification, string operationType)
        {
            // todo: Validate that operationType sent is a valid operation type.
            // todo: validate junanotification
            var fcmData = new
            {
                to = $"/topics/JunaUser-{boardInviteNotification.InviteeUserId}",
                data = boardInviteNotification
            };
            var json = JsonConvert.SerializeObject(fcmData, defaultSerializerSettings);
            var size = json.Length;
            var content = new StringContent(JsonConvert.SerializeObject(fcmData), Encoding.UTF8, "application/json");

            var result = await fcmClient.PostAsync("", content);
            if (result.IsSuccessStatusCode)
            {
                logger.TrackTrace($"Successfully sent push notification");
            }
            else
            {
                logger.TrackTrace($"Received error code {result.StatusCode} when trying to send push notification");
            }
            logger.TrackTrace($"Received the following response body from FCM");
            logger.TrackTrace($"===============================================");
            logger.TrackTrace($"Response body {result.Content.ReadAsStringAsync().Result}");
            logger.TrackTrace($"===============================================");
        }

        public async Task SendBoardLiveData(LiveEvent liveEvent)
        {
            // todo: Validate that operationType sent is a valid operation type.

            // todo: validate junanotification
             var fcmData = new
            {
                to = $"/topics/{liveEvent.BoardTopic}",
                data = liveEvent
            };
            var json = JsonConvert.SerializeObject(fcmData, defaultSerializerSettings);
            var size = json.Length;
            var content = new StringContent(JsonConvert.SerializeObject(fcmData), Encoding.UTF8, "application/json");
            var result = await fcmClient.PostAsync("", content);
            if (result.IsSuccessStatusCode)
            {
                logger.TrackTrace($"Successfully sent FCM broadcast with data =>/n{liveEvent}");
            }
            else
            {
                logger.TrackTrace($"Received error code {result.StatusCode} when trying to send push notification");
            }
            logger.TrackTrace($"Received the following response body from FCM");
            logger.TrackTrace($"===============================================");
            logger.TrackTrace($"Response body { result.Content.ReadAsStringAsync().Result}");
            logger.TrackTrace($"===============================================");
        }
    }
}
