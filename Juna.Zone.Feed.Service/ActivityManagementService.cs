using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Juna.Feed.DomainModel;
using Juna.Feed.DomainModel.Builder;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Util;
using Juna.Feed.Service.Helpers;

namespace Juna.FeedFlows.DomainModel.Service
{
    public class ActivityManagementService
    {
        private ActivityRepository _activityRepository;
        private BoardRepository _boardRepository;
        private FeedItemRepository _feedItemRepository;
        private JunaUserRepository _junaUserRepository;
        private Stream.StreamClient _streamClient;

        public ActivityManagementService(ActivityRepository activityRepository,
            BoardRepository boardRepository,
            FeedItemRepository feedItemRepository,
            JunaUserRepository junaUserRepository,
           Stream.StreamClient streamClient )
        {
            _activityRepository = activityRepository;
            _boardRepository = boardRepository;
            _feedItemRepository = feedItemRepository;
            _junaUserRepository = junaUserRepository;
            _streamClient = streamClient;
        }

        // todo: Find a way to make this async
        public Activity StoreUniqueActivity(Activity activity)
        {
            switch(activity.Verb)
            {
                case "like":
                case "pin":
                case "dislike":
                    var storedItem = _activityRepository.GetByActorVerbAndObject(activity.Actor, activity.Verb, activity.Object);
                    if (storedItem != null) throw new DuplicateEntityException($"activity with actor [{activity.Actor}], verb [{activity.Verb}] and Object [{activity.Object}] already exists");
                    return _activityRepository.Save(activity);
                default:
                    return _activityRepository.Save(activity);
            }
            
        }

        public async Task<Activity> StoreUniqueActivityAsync(Activity activity)
        {
            switch (activity.Verb)
            {
                case "like":
                case "pin":
                case "dislike":
                    var storedItem = _activityRepository.GetByActorVerbAndObject(activity.Actor, activity.Verb, activity.Object);
                    if (storedItem != null) throw new DuplicateEntityException($"activity with actor [{activity.Actor}], verb [{activity.Verb}] and Object [{activity.Object}] already exists");
                    return await _activityRepository.SaveAsync(activity);
                default:
                    return await _activityRepository.SaveAsync(activity);
            }

        }

        public void DeleteActivity(Guid activityId)
        {
            var activity = _activityRepository.GetById(activityId);

            if (activity != null)
            {
                var boardFeed = _streamClient.Feed(FeedGroup.UserFeedType, StreamHelper.GetStreamActorId(activity));
                boardFeed.RemoveActivity(activity.Id.ToString(), true);
                _activityRepository.Delete(activity);
            }
        }

        public void LikeFeedItem(FeedItem feedItem, JunaUser user, string target, string targetId, string time)
        {
            var activity = new Activity();
            switch (target)
            {
                case "Board":
                    var board = _boardRepository.GetById(Guid.Parse(targetId));
                    if (board == null)
                        break;
                    activity = new ActivityBuilder()
                                   .WithActor(user)
                                   .WithVerb(BoardInteractionMetadata.INTERACTION_LIKE)
                                   .WithObject(feedItem)
                                   .WithTarget(board)
                                   .WithTime(DateTime.Parse(time))
                                   .Build();
                    var boardActivity = StoreUniqueActivity(activity);
                    var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(activity));
                    var streamActivity = new Stream.Activity(boardActivity.Actor, boardActivity.Verb, boardActivity.Object)
                    {
                        Target = boardActivity.Target,
                        ForeignId = boardActivity.Id.ToString(),
                    };
                    boardFeed.AddActivity(streamActivity);

                    //delete the dislike activity if user has one
                    var dislikeActivity = _activityRepository.GetByActorVerbObjectandTarget(
                        actor: ActivityHelper.GetActor(user),
                        verb: InteractionMetadata.INTERACTION_DISLIKE,
                        objectString: ActivityHelper.GetObject(feedItem),
                        target: ActivityHelper.GetTarget(board));
                    if(dislikeActivity !=null)
                    {
                        UndoDisLike(feedItem, user);
                    }
                    break;
                case "user":
                    activity = new ActivityBuilder()
                                   .WithActor(user)
                                   .WithVerb(BoardInteractionMetadata.INTERACTION_LIKE)
                                   .WithObject(feedItem)
                                   .WithTime(DateTime.Parse(time))
                                   .Build();
                    var userActivity = StoreUniqueActivity(activity);
                    var userFeed = _streamClient.Feed(FeedGroup.UserFeedType, StreamHelper.GetStreamActorId(activity));
                    streamActivity = new Stream.Activity(userActivity.Actor, userActivity.Verb, userActivity.Object)
                    {
                        Target = userActivity.Target,
                        ForeignId = userActivity.Id.ToString(),
                    };
                    userFeed.AddActivity(streamActivity);
                    break;
                case "Card":
                    //Not implemented yet
                    break;
                default:
                    break;
            }
            // Move this to a database trigger or a change feed processor
            if (feedItem.Interactions == null)
                feedItem.Interactions = new InteractionMetadata
                {
                    Likes = 0,
                    Shares = 0,
                    Pins = 0
                };

            feedItem.Interactions.Likes++;
            _feedItemRepository.Upsert(feedItem);

        }
        public void UnLike(FeedItem feedItem, JunaUser user)
        {
            if (feedItem != null && user != null)
            {
                var activity = _activityRepository.GetByActorVerbAndObject(
                    actor: $"JunaUser:{user.ObjectId}",
                    verb: InteractionMetadata.INTERACTION_LIKE,
                    objectString: $"{feedItem.ContentType}:{feedItem.Id}"
                    );
                if (activity != null)
                {
                    var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(activity));
                    boardFeed.RemoveActivity(activity.Id.ToString(), true);
                    _activityRepository.Delete(activity);
                }
                if (feedItem.Interactions.Likes > 0)
                    feedItem.Interactions.Likes--;

                _feedItemRepository.Upsert(feedItem);
            }
        }

        public void DislikeFeedItem(FeedItem feedItem, JunaUser user, string target, string targetId, string time)
        {
            var activity = new Activity();
            switch (target)
            {
                case "Board":
                    var board = _boardRepository.GetById(Guid.Parse(targetId));
                    if (board == null)
                        break;
                    activity = new ActivityBuilder()
                                   .WithActor(user)
                                   .WithVerb(BoardInteractionMetadata.INTERACTION_DISLIKE)
                                   .WithObject(feedItem)
                                   .WithTarget(board)
                                   .WithTime(DateTime.Parse(time))
                                   .Build();
                    var boardActivity = StoreUniqueActivity(activity);
                    var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(boardActivity));
                    var streamActivity = new Stream.Activity(boardActivity.Actor, boardActivity.Verb, boardActivity.Object)
                    {
                        Target = boardActivity.Target,
                        ForeignId = boardActivity.Id.ToString(),
                    };
                    boardFeed.AddActivity(streamActivity);
                    //delete the like activity if user has one
                    var likeActivity = _activityRepository.GetByActorVerbObjectandTarget(
                        actor: ActivityHelper.GetActor(user),
                        verb: InteractionMetadata.INTERACTION_LIKE,
                        objectString: ActivityHelper.GetObject(feedItem),
                        target: ActivityHelper.GetTarget(board));
                    if (likeActivity != null)
                    {
                        UnLike(feedItem, user);
                    }
                    break;
                case "user":
                    activity = new ActivityBuilder()
                                       .WithActor(user)
                                       .WithVerb(BoardInteractionMetadata.INTERACTION_DISLIKE)
                                       .WithObject(feedItem)
                                       .WithTime(DateTime.Parse(time))
                                       .Build();
                    var userActivity = StoreUniqueActivity(activity);
                    var userFeed = _streamClient.Feed(FeedGroup.UserFeedType, StreamHelper.GetStreamActorId(userActivity));
                    streamActivity = new Stream.Activity(userActivity.Actor, userActivity.Verb, userActivity.Object)
                    {
                        Target = userActivity.Target,
                        ForeignId = userActivity.Id.ToString(),
                    };
                    userFeed.AddActivity(streamActivity);
                    break;
                case "Card":
                    //Not implemented yet
                    break;
                default:
                    break;
            }
            // Move this to a database trigger or a change feed processor
            if (feedItem.Interactions == null)
                feedItem.Interactions = new InteractionMetadata
                {
                    Likes = 0,
                    Shares = 0,
                    Pins = 0,
                    Dislikes = 0
                };

            feedItem.Interactions.Dislikes++;
            _feedItemRepository.Upsert(feedItem);
        }

        public void UndoDisLike(FeedItem feedItem, JunaUser user)
        {
            if (feedItem != null && user != null)
            {
                var activity = _activityRepository.GetByActorVerbAndObject(
                    actor: $"JunaUser:{user.ObjectId}",
                    verb: InteractionMetadata.INTERACTION_DISLIKE,
                    objectString: $"{feedItem.ContentType}:{feedItem.Id}"
                    );
                if (activity != null)
                {
                    var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(activity));
                    boardFeed.RemoveActivity(activity.Id.ToString(), true);
                    _activityRepository.Delete(activity);
                }
                if (feedItem.Interactions.Dislikes > 0)
                    feedItem.Interactions.Dislikes--;

                _feedItemRepository.Upsert(feedItem);
            }
        }

        public void ShareFeedItem(Guid feedItemId, string shareTo, Guid boardId, string userId, string time)
        {
            if (feedItemId != null &&
                shareTo != null &&
                boardId != null &&
                userId != null)
            {
                var feedItem = _feedItemRepository.GetById(feedItemId);
                var board = _boardRepository.GetById(boardId);
                var user = _junaUserRepository.GetByObjectId(userId);
                var activity = new ActivityBuilder()
                                    .WithActor(user)
                                    .WithVerb(BoardInteractionMetadata.INTERACTION_SHARE)
                                    .WithObject(feedItem)
                                    .WithTarget(board)
                                    .WithTime(DateTime.Parse(time))
                                    .Build();
                var shareActivity = _activityRepository.Save(activity);
                var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(shareActivity));
                var streamActivity = new Stream.Activity(shareActivity.Actor, shareActivity.Verb, shareActivity.Object)
                {
                    Target = shareActivity.Target,
                    ForeignId = shareActivity.Id.ToString(),
                };
                boardFeed.AddActivity(streamActivity);
                // Move this to a database trigger or a change feed processor
                if (feedItem.Interactions == null)
                    feedItem.Interactions = new InteractionMetadata
                    {
                        Likes = 0,
                        Shares = 0,
                        Pins = 0
                    };

                feedItem.Interactions.Shares++;
                _feedItemRepository.Upsert(feedItem);
            }
        }

        public void UnshareFeedItem(Guid feedItemId, string userId)
        {
            if (feedItemId != null &&
             userId != null)
            {
                var feedItem = _feedItemRepository.GetById(feedItemId);
                //TODO: Fix issue in deleting a particular activity in Stream.
                // if there are more than on share activities in repository and stream, we should be able 
                // to delete an activity of our choice.
                var user = _junaUserRepository.GetByObjectId(userId);
                var activity = _activityRepository.GetByActorVerbAndObject(
                 actor: $"JunaUser:{user.ObjectId}",
                 verb: InteractionMetadata.INTERACTION_SHARE,
                 objectString: $"{feedItem.ContentType}:{feedItem.Id}"
                 );
                if (activity != null)
                {
                    var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(activity));
                    boardFeed.RemoveActivity(activity.Id.ToString(), true);
                    _activityRepository.Delete(activity);
                }
                if (feedItem.Interactions.Shares > 0)
                    feedItem.Interactions.Shares--;

                _feedItemRepository.Upsert(feedItem);
            }
        }
      
        public void PinFeedItem(FeedItem feedItem, JunaUser user, string target, string targetId, string time)
        {
            var activity = new Activity();
            switch(target)
            {
                case "Board":
                    var board = _boardRepository.GetById(Guid.Parse(targetId));
                    if (board == null) break;
                    activity = new ActivityBuilder()
                            .WithActor(user)
                            .WithVerb(BoardInteractionMetadata.INTERACTION_PIN)
                            .WithObject(feedItem)
                            .WithTarget(board)
                            .WithTime(DateTime.Parse(time))
                            .Build();
                    var pinActivity = StoreUniqueActivity(activity);
                    var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(pinActivity));
                    var streamActivity = new Stream.Activity(pinActivity.Actor, pinActivity.Verb, pinActivity.Object)
                    {
                        Target = pinActivity.Target,
                        ForeignId = pinActivity.Id.ToString()
                    };
                    boardFeed.AddActivity(streamActivity);
                    break;
                case "user":
                    var targetUser = _junaUserRepository.GetByObjectId(targetId);
                    if (targetUser == null) break;
                    activity = new ActivityBuilder()
                            .WithActor(user)
                            .WithVerb(BoardInteractionMetadata.INTERACTION_PIN)
                            .WithObject(feedItem)
                            .WithTarget(targetUser)
                            .WithTime(DateTime.Parse(time))
                            .Build();
                    var userPinActivity = StoreUniqueActivity(activity);
                    var userFeed = _streamClient.Feed(FeedGroup.UserFeedType, StreamHelper.GetStreamActorId(userPinActivity));
                    var streamPinActivity = new Stream.Activity(userPinActivity.Actor, userPinActivity.Verb, userPinActivity.Object)
                    {
                        Target = userPinActivity.Target,
                        ForeignId = userPinActivity.Id.ToString()
                    };
                    userFeed.AddActivity(streamPinActivity);
                    break;
                case "Card":
                    //to be implemented
                    break;
                default:
                    break;
            }

            if (feedItem.Interactions == null)
                feedItem.Interactions = new InteractionMetadata
                {
                    Likes = 0,
                    Shares = 0,
                    Pins = 0
                };
            feedItem.Interactions.Pins++;
            _feedItemRepository.Upsert(feedItem);
        }

        public void DeletePin(FeedItem feedItem, string pinId)
        {
            var activity = _activityRepository.GetById(Guid.Parse(pinId));
            if (activity != null)
            {
                _activityRepository.Delete(activity);
                if (feedItem.Interactions.Pins > 0)
                    feedItem.Interactions.Pins--;
                _feedItemRepository.Upsert(feedItem);
            }
        }
        
       
    }
}

