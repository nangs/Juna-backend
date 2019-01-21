using Juna.Feed.DomainModel;
using Juna.Feed.Repository;
using Juna.Feed.Service.Helpers;
using Juna.FeedFlows.DomainModel.Service;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Juna.Feed.Service
{
    public class ContentUploadService
    {
		private FeedItemRepository _feedItemRepository;
		private ThumbnailService _thumbnailService;
		private FCMSenderService _fcmSenderService;
		private readonly StorageCredentials _storageCredentials;
        private ActivityManagementService _activityService;
        private BoardRepository _boardsRepository;
        private BlobHelper _blobHelper;
        private Stream.StreamClient _streamClient; 

		public ContentUploadService(
			FeedItemRepository feedItemRepository,
			ThumbnailService thumbnailService,
			FCMSenderService fcmSenderService, 
			StorageCredentials storageCredentials,
            ActivityManagementService activityService,
            BoardRepository boardsRepository,
            BlobHelper blobHelper,
            Stream.StreamClient streamClient)
		{
			_feedItemRepository = feedItemRepository;
			_thumbnailService = thumbnailService;
			_fcmSenderService = fcmSenderService;
			_storageCredentials = storageCredentials;
            _activityService = activityService;
            _boardsRepository = boardsRepository;
            _blobHelper = blobHelper;
            _streamClient = streamClient;
		}

        public async Task UploadFile(
            FeedItem feedItem,
            JunaUser user,
            string mimeType,
            System.IO.Stream fileStream,
            string targetType,
            string description) => await UploadFile(feedItem, user, mimeType, fileStream, targetType, null, description);

        public async Task UploadFile(
			FeedItem feedItem,
			JunaUser user,
			string mimeType, 
			System.IO.Stream fileStream,
            string targetType,
            Board board,
            string description)
		{
			switch (mimeType)
			{
				case ("image/jpeg"):
				case ("image/png"):
				case ("image/gif"):
				case ("image/bmp"):
					await UploadAndSaveFeedItemAsync(feedItem,
						FeedItem.ImageFeedItem, fileStream,
						user, mimeType , targetType , board , description);
					break;

				case ("video/mp4"):
					await UploadAndSaveFeedItemAsync(feedItem,
						FeedItem.VideoFeedItem, fileStream,
						user, mimeType, targetType, board, description);
					break;

				case ("audio/mpeg"):
					await UploadAndSaveFeedItemAsync(feedItem,
						FeedItem.AudioFeedItem, fileStream,
						user, mimeType, targetType, board, description);
					break;

				default:
					throw new InvalidOperationException("Cannot process this file type");
			}
		}

        private async Task UploadAndSaveFeedItemAsync(FeedItem feedItem,
            string feedItemType, System.IO.Stream fileStream,
            JunaUser user, string mimeType, string targetType, Board board , string description)
        {
            feedItem.ContentType = feedItemType;
            feedItem.Actor = user;
            feedItem.Description = description;
            feedItem.Url = _blobHelper.GenerateFilePath(guid: feedItem.Id, itemType: feedItemType, username: user.Id.ToString());
            var blob = new CloudBlockBlob(new Uri(feedItem.Url), _storageCredentials);
            await blob.UploadFromStreamAsync(fileStream);
            var thumbnailUrl = _blobHelper.GenerateThumbnailFilePath(guid: feedItem.Id, itemType: feedItemType, username: user.Id.ToString());

            await _thumbnailService.GenerateThumbnail(feedItem.Url, thumbnailUrl, mimeType, feedItem.Title);

            feedItem.DatePublished = DateTime.UtcNow;
            feedItem.Thumbnail = new Image
            {
                // todo: these two should be read from config
                ImageHeight = 300,
                ImageWidth = 300,
                ImageUrl = thumbnailUrl
            };
            _feedItemRepository.Upsert(feedItem);
            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Actor = $"JunaUser:{user.ObjectId}",
                Time = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                Object = $"{feedItem.ContentType}:{feedItem.Id}",
            };
            switch (targetType)
            {
                case (FeedGroup.BoardFeedType):
                    activity.Target = ActivityHelper.GetTarget(board);
                    activity.Verb = InteractionMetadata.INTERACTION_POST;
					var postActivity = _activityService.StoreUniqueActivity(activity);
                    var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(postActivity));
                    var streamActivity = new Stream.Activity(actor: postActivity.Actor, verb: postActivity.Verb, @object: postActivity.Object)
                    {
                        Target = postActivity.Target,
                        ForeignId = postActivity.Id.ToString(),
                    };
                  await boardFeed.AddActivity(streamActivity);
                    if (board.Interactions == null)
                    {
                        board.Interactions = new BoardInteractionMetadata
                        {
                            Likes = 0,
                            Shares = 0,
                            Comments = 0,
                            Followers = 0,
                            Pins = 0,
                            Posts = 0
                        };
                    }
                    board.Interactions.Posts++;
                    _boardsRepository.Upsert(board);

					// todo: Find a better way to send it than dynamic formatting like this. use a mapper and a transfer object
					await _fcmSenderService.SendFcmBoardNotification(
						new JunaNotification
						{
							Title = feedItem.Title,
							Actor = user.DisplayName,
							Action = activity.Verb,
							ImageUrl = feedItem.Url,
							ThumbnailImageUrl = feedItem.Thumbnail.ImageUrl,
							ThumbnailWidth = feedItem.Thumbnail.ImageWidth,
							ThumbnailHeight = feedItem.Thumbnail.ImageHeight,
							ContentType = feedItem.ContentType,
                            ForeignId = board.BoardType.Equals("private") ? 0 : board.BoardEvent.ForeignId

                        },
						board,
						FCMSenderService.CREATE_OPERATION);
					break;
                case (FeedGroup.CardFeedType):
					_activityService.StoreUniqueActivity(new Activity
					{
						Actor = ActivityHelper.GetActor(user),
						Verb = InteractionMetadata.INTERACTION_POST,
						Object = ActivityHelper.GetObject(feedItem),
						Target = StreamHelper.GetCardTarget(user),
                        Time = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                    });
                    feedItem.Interactions.Posts++;
                    break;

                case (FeedGroup.UserFeedType):
                    activity.Verb = InteractionMetadata.INTERACTION_POST;
                    var userPostActivity = _activityService.StoreUniqueActivity(activity);
                    var userFeed = _streamClient.Feed(FeedGroup.UserFeedType, StreamHelper.GetStreamActorId(userPostActivity));
                    var userStreamActivity = new Stream.Activity(actor: userPostActivity.Actor, verb: userPostActivity.Verb, @object: userPostActivity.Object)
                    {
                        ForeignId = $"{userPostActivity.Id}",
                    };
                    await userFeed.AddActivity(userStreamActivity);
                    if (feedItem.Interactions == null)
                    {
                        feedItem.Interactions = new InteractionMetadata
                        {
                            Likes = 0,
                            Shares = 0,
                            Comments = 0,
                            Pins = 0,
                            Posts = 0
                        };
                    }
                    feedItem.Interactions.Posts++;
                    _feedItemRepository.Upsert(feedItem);
                    break;
            }
        }
    }
}
