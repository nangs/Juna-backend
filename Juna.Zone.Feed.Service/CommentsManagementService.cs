using System;
using System.Collections.Generic;
using System.Linq;
using Juna.Feed.DomainModel;
using Juna.Feed.DomainModel.Builder;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Util;
using Juna.Feed.Service.Helpers;

namespace Juna.Feed.Service
{
    public class CommentsManagementService
    {
        private CommentsRepository _commentsRepository;
        private FeedItemRepository _feedItemRepository;
        private Stream.StreamClient _streamClient;
        public CommentsManagementService(CommentsRepository commentRepository,
            FeedItemRepository feedItemRepository,
            Stream.StreamClient streamClient)
        {
            _commentsRepository = commentRepository;
            _feedItemRepository = feedItemRepository;
            _streamClient = streamClient;
        }

        public FeedItem GetCommentsByFeedItem(Guid feedItemId)
        {
            var feedItem = new FeedItem(string.Empty);
            var comments = new List<Comment>();

            if (feedItemId != null)
            {
                feedItem = _feedItemRepository.GetById(feedItemId);
                comments = _commentsRepository.GetByFeedItemAndVerb(feedItem, "comment");

                if (comments != null)
                {
                    feedItem.Comments = comments;
                }
            }
            return feedItem;
        }

        // todo: Find a way to make this async
        public Comment StoreComments(Board board, FeedItem feedItem, JunaUser user, string timeStamp, string comment)
        {
            var activity = new CommentBuilder()
                            .WithActor(user)
                            .WithVerb(BoardInteractionMetadata.INTERACTION_COMMENT)
                            .WithObject(feedItem)
                            .WithTarget(board)
                            .WithMessage(comment)
                            .WithTime(DateTime.Parse(timeStamp))
                            .Build();
            var commentActivity = _commentsRepository.Save(activity);
            var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(commentActivity));
            var streamActivity = new Stream.Activity(commentActivity.Actor, commentActivity.Verb, commentActivity.Object)
            {
                Target = commentActivity.Target,
                ForeignId = commentActivity.Id.ToString(),
            };
            streamActivity.SetData("Message", commentActivity.Message);
            boardFeed.AddActivity(streamActivity);
            // Move this to a database trigger or a change feed processor
            if (feedItem.Interactions == null)
                feedItem.Interactions = new InteractionMetadata
                {
                    Likes = 0,
                    Shares = 0,
                    Pins = 0,
                    Comments = 0
                };
            feedItem.Interactions.Comments++;
            _feedItemRepository.Upsert(feedItem);
            return commentActivity;
        }

        public Comment ReplyToComment(Board board, FeedItem feedItem, JunaUser user, string timeStamp, string replyComment, Comment parentComment)
        {
            var activity = new CommentBuilder()
                            .WithActor(user)
                            .WithVerb(BoardInteractionMetadata.INTERACTION_COMMENT)
                            .WithObject(feedItem)
                            .WithTarget(board)
                            .WithTime(DateTime.Parse(timeStamp))
                            .WithParentCommentId(parentComment)
                            .WithMessage(replyComment)
                            .Build();
            var replyCommentActivity = _commentsRepository.Save(activity);
            var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(replyCommentActivity));
            var streamActivity = new Stream.Activity(replyCommentActivity.Actor, replyCommentActivity.Verb, replyCommentActivity.Object)
            {
                Target = replyCommentActivity.Target,
                ForeignId = replyCommentActivity.Id.ToString(),
            };
            streamActivity.SetData("Message", replyCommentActivity.Message);
            boardFeed.AddActivity(streamActivity);
            // Move this to a database trigger or a change feed processor
            if (feedItem.Interactions == null)
                feedItem.Interactions = new InteractionMetadata
                {
                    Likes = 0,
                    Shares = 0,
                    Pins = 0,
                    Comments = 0
                };
            feedItem.Interactions.Comments++;
            _feedItemRepository.Upsert(feedItem);
            return replyCommentActivity;
        }

        public void DeleteComments(FeedItem feedItem, Comment activity)
        {
            if (activity != null)
            {
                var replyActivities = _commentsRepository.GetByParentCommentId(activity.Id);
                if (replyActivities.Count < 0)
                {
                    _commentsRepository.Delete(activity);
                    if (feedItem.Interactions.Comments > 0)
                        feedItem.Interactions.Comments--;
                    _feedItemRepository.Upsert(feedItem);
                }
                else
                {
                    foreach(var reply in replyActivities)
                    {
                        _commentsRepository.Delete(reply);
                        if (feedItem.Interactions.Comments > 0)
                            feedItem.Interactions.Comments--;
                        _feedItemRepository.Upsert(feedItem);
                    }
                    _commentsRepository.Delete(activity);
                    if (feedItem.Interactions.Comments > 0)
                        feedItem.Interactions.Comments--;
                    _feedItemRepository.Upsert(feedItem);
                }
            }
        }
    }
}
