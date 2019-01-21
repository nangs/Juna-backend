using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Juna.Feed.DomainModel;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Util;
using Juna.Feed.DomainModel.Builder;
using Juna.Feed.Service.Helpers;

namespace Juna.Feed.Service
{
    public class BoardManagementService
    {
        private BoardRepository _boardRepository;
		private ActivityRepository _activityRepository;
        private readonly FeedItemRepository _feedItemRepository;
        private JunaUserRepository _userRepository;
        private Stream.StreamClient _streamClient;

        public BoardManagementService(BoardRepository boardsRepository, 
            ActivityRepository activityRepository, 
            FeedItemRepository feedItemRepository,
            JunaUserRepository userRepository,
            Stream.StreamClient streamClient
            )
        {
            _boardRepository = boardsRepository;
			_activityRepository = activityRepository;
            _feedItemRepository = feedItemRepository;
            _userRepository = userRepository;
            _streamClient = streamClient;
        }

		public Board CreateBoard(Board board)
		{
			if (board.BoardEvent == null || board.BoardEvent.ForeignId.Equals("")) throw new InvalidOperationException();
			var existingBoard = _boardRepository.GetByBoardEventAndForeignId(board.BoardEvent , board.BoardEvent.ForeignId );
			if (existingBoard == null)
			{
				return _boardRepository.Save(board);
			}
            else
            throw new DuplicateEntityException($"Board with Event Type  [{board.BoardEvent.Type}] and ForeignId [{board.BoardEvent.ForeignId}] already exists");
        }

        public Board CreatePrivateBoard(Board board)
        {
                return _boardRepository.Save(board); 
        }

        // todo: Find a way to make this async
        public Board StoreBoardWithUniqueId(Board board)
        {
            var storedItem = _boardRepository.GetById(board.Id);
            if (storedItem != null) throw new DuplicateEntityException($"board with Id [{board.Id}] already exists");

            return _boardRepository.Save(board);
        }

		public Board UserEntersBoard(JunaUser user, string timestamp, Board board)
		{
            var activity = new ActivityBuilder()
                            .WithActor(user)
                            .WithVerb(BoardInteractionMetadata.BOARD_INTERACTION_ENTER)
                            .WithObject(board)
                            //  .WithForeignId(user)
                            //TODO: this is a bug. Upon entering a Board the Object is set as "JunaUser: userObjectId"
                            // it should be Board-BoardId
                            // when .WithForeignId(user) mwthod is called the previous value is overwritten and we are getting object as "JunaUser: userObjectId"
                            .WithTime(DateTime.Parse(timestamp))
                            .Build();
            var enterBoardActivity = _activityRepository.Save(activity);
            var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(enterBoardActivity));
            var streamActivity = new Stream.Activity(enterBoardActivity.Actor, enterBoardActivity.Verb, enterBoardActivity.Object)
            {
                Target = enterBoardActivity.Target,
                ForeignId = enterBoardActivity.Id.ToString(),
            };
            boardFeed.AddActivity(streamActivity);


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
                    ActiveUsers=0
                    
				};
			}
			// todo: This is not accurate or thread-safe. Also, multiple entries are counted as multiple users
			board.Interactions.Followers++;
            board.Interactions.ActiveUsers++;

			return _boardRepository.Upsert(board);
		}

        public Board UserExitBoard(JunaUser user, string timestamp, Board board)
        {
            var activity = _activityRepository.GetByActorVerbAndObject(
                 actor: $"JunaUser:{user.Id}",
                 verb: BoardInteractionMetadata.BOARD_INTERACTION_ENTER,
                 objectString: $"Board:{board.Id}"
                 );

            if (activity != null)
            {
                var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(activity));
                boardFeed.RemoveActivity(activity.Id.ToString(), true);
                _activityRepository.Delete(activity);
            }
            if (board.Interactions.ActiveUsers > 0)
            {
                board.Interactions.ActiveUsers--;
            }

            return _boardRepository.Upsert(board);
        }

        public Board UserFollowsBoard(JunaUser user, string timestamp, Board board)
        {
            var activity = new ActivityBuilder()
                            .WithActor(user)
                            .WithVerb(InteractionMetadata.INTERACTION_FOLLOW)
                            .WithObject(board)
                        //  .WithForeignId(user)
                      //TODO: this is a bug. Upon following a Board the Object is set as "JunaUser: userObjectId"
                     // it should be Board-BoardId
                     // when .WithForeignId(user) mwthod is called the previous value is overwritten and we are getting object as "JunaUser: userObjectId"
                            .WithTime(DateTime.Parse(timestamp))
                            .Build();
            var followBoardActivity = _activityRepository.Save(activity);
            var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(followBoardActivity));
            var streamActivity = new Stream.Activity(followBoardActivity.Actor, followBoardActivity.Verb, followBoardActivity.Object)
            {
                Target = followBoardActivity.Target,
                ForeignId = followBoardActivity.Id.ToString(),
            };
            boardFeed.AddActivity(streamActivity);

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
            // todo: This is not accurate or thread-safe. Also, multiple entries are counted as multiple users
            board.Interactions.Followers++;
            return _boardRepository.Upsert(board);
        }

        public Board UserUnFollowsBoard(JunaUser user, string timestamp, Board board)
        {
            var activity = _activityRepository.GetByActorVerbAndObject(
                     actor: $"JunaUser:{user.Id}",
                     verb: InteractionMetadata.INTERACTION_FOLLOW,
                     objectString: $"Board:{board.Id}"
                     );
            if (activity != null)
            {
                var boardFeed = _streamClient.Feed(FeedGroup.BoardFeedType, StreamHelper.GetStreamActorId(activity));
                boardFeed.RemoveActivity(activity.Id.ToString(), true);
                _activityRepository.Delete(activity);
            }
            if(board.Interactions.Followers>0)
                board.Interactions.Followers--;
            // todo: This is not accurate or thread-safe. Also, multiple entries are counted as multiple users
            return _boardRepository.Upsert(board);
        }

        public Board GetBoard(Guid boardId)
        {
            var board = new Board();
            if(boardId != null)
            {
                board = _boardRepository.GetById(boardId);
            }
            return board;
        }

        public List<Board> GetByDate(string date)
        {
            var board = new List<Board>();
            if (date != null)
            {
                board = _boardRepository.GetByDate(date);
            }
            return board;
        }

        public List<Board> GetBoardsThatUserIsPartOf(JunaUser user)
        {
            var selectedBoardsList = new List<Board>();
            if (user != null)
            {
                var createdBoards = _boardRepository.GetBoardsCreatedByUser(user);
                selectedBoardsList.AddRange(createdBoards);
                //boards that user is following
                var boards = _boardRepository.GetAllBoards();
                foreach(var board in boards)
                {
                    var followActivity = _activityRepository.GetByActorVerbAndObject($"JunaUser:{user.ObjectId}", BoardInteractionMetadata.INTERACTION_FOLLOW, $"Board-{board.Id}");
                    if (followActivity != null)
                    {
                        var selectedBoard = _boardRepository.GetById(Guid.Parse(followActivity.Object.Split(new char[] { '-' }, 2)[1]));
                        selectedBoardsList.Add(selectedBoard);
                    }
                }
            }
            return selectedBoardsList;
        }

        public List<JunaUser> GetAllMembersOfBoard(JunaUser user, Board board)
        {
            var memberList = new List<JunaUser>();
            if (user != null && board != null)
            {
                var creator = board.CreatedBy;
                memberList.Add(creator);
                //members that are following
                var followActivities = _activityRepository.GetByVerbAndObject(BoardInteractionMetadata.INTERACTION_FOLLOW, $"Board-{board.Id}");
                foreach (var followActivity in followActivities)
                {
                    var followerId = followActivity.Actor.Split(new char[] { ':' }, 2)[1];
                    var follower = _userRepository.GetByObjectId(followerId);
                    if(!memberList.Contains(follower))
                    memberList.Add(follower);
                }
            }
            return memberList;
        }

        public Board GetByBoardEvent(long foreignId, string boardType)
        {
            var board = new Board();
            board = _boardRepository.GetByBoardEvent(new BoardEvent
            {
                ForeignId = foreignId,
                Type = boardType
            });
            return board;
        }
    }
}
