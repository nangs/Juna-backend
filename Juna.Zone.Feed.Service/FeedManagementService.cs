using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Juna.Feed.DomainModel;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Util;
using Juna.Feed.Service;
using System.Net;
using Juna.Feed.DomainModel.Builder;
using Juna.Feed.Service.Helpers;

namespace Juna.FeedFlows.DomainModel.Service
{
	public class FeedManagementService
    {
        public const string FOOTBALL_BOT = "Juna-Football";
        private FeedItemRepository _feedItemRepository;
        private BoardRepository _boardRepository;
        private ActivityRepository _activityRepository;
        private JunaUserRepository _junaUserRepository;
        private readonly ActivityManagementService _activityManagementService;
        private FCMSenderService _fcmSenderService;
        private TelemetryClient logger;
        private Stream.StreamClient _streamClient;

        public FeedManagementService(FeedItemRepository feedItemRepository,
            BoardRepository boardRepository,
            ActivityRepository activityRepository,
            JunaUserRepository junaUserRepository,
            ActivityManagementService activityManagementService,
            FCMSenderService fCMSenderService,
            TelemetryClient telemetryClient,
            Stream.StreamClient streamClient
            )
		{
            _feedItemRepository = feedItemRepository;
            _boardRepository = boardRepository;
            _activityRepository = activityRepository;
            _junaUserRepository = junaUserRepository;
            _activityManagementService = activityManagementService;
            _fcmSenderService = fCMSenderService;
            logger = telemetryClient;
            _streamClient = streamClient;
        }

		// todo: Find a way to make this async
		public FeedItem StoreItemWithUniqueUrl(FeedItem newsfeedItem)
		{
			var storedItem = _feedItemRepository.GetByUrl(newsfeedItem.Url);
			if (storedItem != null) throw new DuplicateEntityException($"feed item with url [{newsfeedItem.Url}] already exists");

			return _feedItemRepository.Save(newsfeedItem);
		}

        public async Task<FeedItem> StoreItemWithUniqueUrlAsync(FeedItem newsfeedItem)
        {
            var storedItem = _feedItemRepository.GetByUrl(newsfeedItem.Url);
            if (storedItem != null) throw new DuplicateEntityException($"feed item with url [{newsfeedItem.Url}] already exists");

            return await _feedItemRepository.SaveAsync(newsfeedItem);
        }

        public List<FeedItem> GetFeedItems(Guid boardId)
        {
            var feedItems = new List<FeedItem>();
            var objectIdlist = new List<string>();

            var board = _boardRepository.GetById(boardId);

            if (board != null)
            {
                var activity = _activityRepository.GetByBoardId(
                    target: $"Board-{board.Id}"
                    );

                if (activity != null)
                {
                    foreach (var obj in activity)
                    {
                        var objectId = StreamHelper.GetStreamObjectId(obj);
                        objectIdlist.Add(objectId);
                    }
                    feedItems = _feedItemRepository.GetByObjectId(objectIdlist);
                }
            }
            return feedItems;
        }

        public async void CreateFeedItem(Board board, JunaUser user, string contentType, string title, DateTime dateCreated)
        {
            var feedItem = new RootCommentFeedItem()
            {
                Id = Guid.NewGuid(),
                ContentType = contentType,
                Title = title,
                DateCreated = dateCreated,
                Actor = user
            };

            _feedItemRepository.Upsert(feedItem);
              var  activity = new ActivityBuilder()
                                .WithActor(user)
                                .WithVerb(BoardInteractionMetadata.INTERACTION_POST)
                                .WithObject(feedItem)
                                .WithTarget(board)
                                .WithTime(dateCreated)
                                .Build();
            var createFeedItemActivity = _activityRepository.Save(activity);
            var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(createFeedItemActivity));
            var streamActivity = new Stream.Activity(createFeedItemActivity.Actor, createFeedItemActivity.Verb, createFeedItemActivity.Object)
            {
                Target = createFeedItemActivity.Target,
                ForeignId = createFeedItemActivity.Id.ToString(),
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
                    Posts = 0,
                    ActiveUsers = 0

                };
            }

            board.Interactions.Posts++;
            _boardRepository.Upsert(board);
            await _fcmSenderService.SendFcmBoardNotification(
				 new JunaNotification
				 {
					 Title = title,
					 Actor = user.DisplayName,
					 Action = BoardInteractionMetadata.INTERACTION_POST,
					 ContentType = contentType,
                     ForeignId = board.BoardType.Equals("private")?0 : board.BoardEvent.ForeignId
				 },
				 board,
				 FCMSenderService.CREATE_OPERATION);
        }

        public bool DeleteFeedItem(Guid feedItemId, Guid boardId, string userId )
        {
            var feedItem = _feedItemRepository.GetById(feedItemId);
            var board = _boardRepository.GetById(boardId);
            var user = _junaUserRepository.GetByObjectId(userId);

            var activity = _activityRepository.GetByActorVerbObjectandTarget(
              actor: $"JunaUser:{userId}",
              verb: InteractionMetadata.INTERACTION_POST,
              objectString: $"{feedItem.ContentType}:{feedItem.Id}",
              target: $"Board-{board.Id}"
              );

            try
            {
                if (activity != null)
                    _activityRepository.Delete(activity);

                if (board.Interactions.Posts > 0)
                    board.Interactions.Posts--;

                _boardRepository.Upsert(board);
                _feedItemRepository.Upsert(feedItem);

                return true;
            }
            catch (Exception ex)
            {
                if (ex.ToString().Length > 0)
                {
                    return false;
                }
            }

            return false;
        }

        public void CreateFeedItem(List<FeedItem> feedItems)
        {
            if (feedItems != null)
            {
                foreach (FeedItem f in feedItems)
                {
                    ProcessFeedItems(f);
                }
            }
        }
        #region private

        private void ProcessFeedItems(FeedItem feedItem)
        {
            try
            {
                logger.TrackTrace($"Processing feed item with Title [{feedItem.Title}] of type: [{feedItem.ContentType}]", SeverityLevel.Information);
                logger.TrackTrace("\nInserting newsfeed into the database\n", SeverityLevel.Information);

                logger.TrackTrace($"\n New URL is [{feedItem.Title}]", SeverityLevel.Information);
                logger.TrackTrace(JsonConvert.SerializeObject(feedItem), SeverityLevel.Verbose);

                feedItem = StoreItemWithUniqueUrl(feedItem);
                logger.TrackTrace($"Successfully processed feed item of type [{feedItem.ContentType}] with url [{feedItem.Url}]");
            }
            catch (DuplicateEntityException)
            {
                logger.TrackTrace($"FeedItem with the Url [{feedItem.Url}] already exists. Skipping", SeverityLevel.Warning);
            }
            finally
            {
                logger.TrackTrace($"Insert attempt completed for record with headline: [{feedItem.Title}]", SeverityLevel.Information);
            }
        }

        #endregion
    }
}
