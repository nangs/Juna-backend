using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Juna.Feed.DomainModel;
using Juna.Feed.DomainModel.Builder;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Util;
using Juna.Feed.Service.Helpers;
using Juna.FeedFlows.DomainModel.Service;

namespace Juna.Feed.Service
{
    //TODO: To refactor delete api's according to ActivityBuilder and add StreamDelete activity for the remaining
    public class ModerationManagementService
    {
        private readonly JunaUserRepository _junaUserRepository;
        private ActivityRepository _activityRepository;
        private readonly ActivityManagementService _activityService;
        private Stream.StreamClient _streamClient;

        public ModerationManagementService(ActivityRepository activityRepository,
            JunaUserRepository junaUserRepository,
            ActivityManagementService activityService,
            Stream.StreamClient streamClient)
        {
            _junaUserRepository = junaUserRepository;
            _activityRepository = activityRepository;
            _activityService = activityService;
            _streamClient = streamClient;
        }
        public Activity BlockUser(JunaUser user, JunaUser blockUser, string time)
        {
            var activity = new ActivityBuilder()
                            .WithActor(user)
                            .WithVerb(BoardInteractionMetadata.INTERACTION_BLOCK)
                            .WithObject(blockUser)
                            .WithTime(DateTime.Parse(time))
                            .Build();
            var blockActivity = _activityRepository.Save(activity);
            var userFeed = _streamClient.Feed(FeedGroup.UserFeedType, StreamHelper.GetStreamActorId(blockActivity));
            var streamActivity = new Stream.Activity(blockActivity.Actor, blockActivity.Verb, blockActivity.Object)
            {
                Target = blockActivity.Target,
                ForeignId = blockActivity.Id.ToString()
            };
            userFeed.AddActivity(streamActivity);
            return blockActivity;
        }

        public void UnBlock(JunaUser user, JunaUser blockedUser)
        {
            var activity = _activityRepository.GetByActorVerbAndObject(
                       actor: ActivityHelper.GetActor(user),
                       verb: InteractionMetadata.INTERACTION_BLOCK,
                       objectString: ActivityHelper.GetObject(blockedUser)
                    );
            var boardFeed = _streamClient.Feed(FeedGroup.UserFeedType, StreamHelper.GetStreamActorId(activity));
            boardFeed.RemoveActivity(activity.Id.ToString(), true);
            if (activity != null)
                _activityRepository.Delete(activity);
        }
        public Activity BanUser(JunaUser user, JunaUser banUser, Board board, string time)
        {
            var activity = new ActivityBuilder()
                            .WithActor(user)
                            .WithVerb(BoardInteractionMetadata.INTERACTION_BAN)
                            .WithObject(banUser)
                            .WithTarget(board)
                            .WithTime(DateTime.Parse(time))
                            .Build();
            var banActivity = _activityRepository.Save(activity);
            var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(banActivity));
            var streamActivity = new Stream.Activity(banActivity.Actor, banActivity.Verb, banActivity.Object)
            {
                Target = banActivity.Target,
                ForeignId = banActivity.Id.ToString()
            };
            boardFeed.AddActivity(streamActivity);
            return banActivity;
        }

        public void UnBanUser(Board board, JunaUser user, JunaUser bannedUser)
        {
            var activity = _activityRepository.GetByActorVerbObjectandTarget(
                        actor: ActivityHelper.GetActor(user),
                        verb: InteractionMetadata.INTERACTION_BAN,
                        objectString: ActivityHelper.GetObject(bannedUser),
                        target: ActivityHelper.GetTarget(board)
                     );
            if (activity != null)
            {
                var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(activity));
                boardFeed.RemoveActivity(activity.Id.ToString(), true);
                _activityRepository.Delete(activity);
            }
        }

        public Activity MuteUser(JunaUser user, JunaUser muteUser, Board board, string time)
        {
            var activity = new ActivityBuilder()
                            .WithActor(user)
                            .WithVerb(BoardInteractionMetadata.INTERACTION_MUTE)
                            .WithObject(muteUser)
                            .WithTarget(board)
                            .WithTime(DateTime.Parse(time))
                            .Build();
            var muteActivity = _activityRepository.Save(activity);
            var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(muteActivity));
            var streamActivity = new Stream.Activity(muteActivity.Actor, muteActivity.Verb, muteActivity.Object)
            {
                Target = muteActivity.Target,
                ForeignId = muteActivity.Id.ToString()
            };
            boardFeed.AddActivity(streamActivity);
            return muteActivity;
        }

        public void UnMuteUser(Board board, JunaUser user, JunaUser mutedUser)
        {
            var activity = _activityRepository.GetByActorVerbObjectandTarget(
                     actor: ActivityHelper.GetActor(user),
                     verb: InteractionMetadata.INTERACTION_MUTE,
                     objectString: ActivityHelper.GetObject(mutedUser),
                     target: ActivityHelper.GetTarget(board)
                  );

            if (activity != null)
            {
                var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(activity));
                boardFeed.RemoveActivity(activity.Id.ToString(), true);
                _activityRepository.Delete(activity);
            }
        }

        public Activity Report(JunaUser reportUser, Comment comment, FeedItem reportFeedItem, JunaUser user, Board board, string time)
        {
            var reportActivity = new Activity();
            if (reportUser != null)
            {
                reportActivity = new ActivityBuilder()
                           .WithActor(user)
                           .WithVerb(BoardInteractionMetadata.INTERACTION_REPORT)
                           .WithObject(reportUser)
                           .WithTarget(board)
                           .WithTime(DateTime.Parse(time))
                           .Build();
                var reportUserActivity = _activityRepository.Save(reportActivity);
                var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(reportUserActivity));
                var streamActivity = new Stream.Activity(reportUserActivity.Actor, reportUserActivity.Verb, reportUserActivity.Object)
                {
                    Target = reportUserActivity.Target,
                    ForeignId = reportUserActivity.Id.ToString()
                };
                boardFeed.AddActivity(streamActivity);
            }
            if (comment != null)
            {
                reportActivity = new ActivityBuilder()
                           .WithActor(user)
                           .WithVerb(BoardInteractionMetadata.INTERACTION_REPORT)
                           .WithObject(comment)
                           .WithTarget(board)
                           .WithTime(DateTime.Parse(time))
                           .Build();
                var reportCommentActivity = _activityRepository.Save(reportActivity);
                var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(reportCommentActivity));
                var streamActivity = new Stream.Activity(reportCommentActivity.Actor, reportCommentActivity.Verb, reportCommentActivity.Object)
                {
                    Target = reportCommentActivity.Target,
                    ForeignId = reportCommentActivity.Id.ToString(),
                };
                boardFeed.AddActivity(streamActivity);
            }

            if (reportFeedItem != null)
            {
                reportActivity = new ActivityBuilder()
                          .WithActor(user)
                          .WithVerb(BoardInteractionMetadata.INTERACTION_REPORT)
                          .WithObject(reportFeedItem)
                          .WithTarget(board)
                          .WithTime(DateTime.Parse(time))
                          .Build();
                var reportFeedItemActivity = _activityRepository.Save(reportActivity);
                var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(reportFeedItemActivity));
                var streamActivity = new Stream.Activity(reportFeedItemActivity.Actor, reportFeedItemActivity.Verb, reportFeedItemActivity.Object)
                {
                    Target = reportFeedItemActivity.Target,
                    ForeignId = reportFeedItemActivity.Id.ToString(),
                };
                boardFeed.AddActivity(streamActivity);
            }
            return reportActivity;
        }
    }
}
