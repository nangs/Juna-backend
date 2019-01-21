using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Juna.Feed.DomainModel;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Util;
using Juna.FeedFlows.DomainModel.Service;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Juna.Feed.Service;
using Newtonsoft.Json;
using System.Security.Claims;
using Juna.Feed.WebApi.Helpers;
using static Juna.Feed.Repository.RepositoryConstants;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Juna.Feed.Service.Helpers;

namespace Juna.Feed.WebApi.Controllers
{
    [Authorize]
    [Route("activities")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private const string FOOTBALL_BOT = "JunaFootball";
        private FeedManagementService _feedService;
        private FeedItemRepository _feedItemRepository;
        private CommentsManagementService _commentsService;
        private CommentsRepository _commentsRepository;
        private ActivityManagementService _activityService;
        private ActivityRepository _activityRepository;
        private JunaUserRepository _userRepository;
        private ContentUploadService _contentUploadService;
        private BoardRepository _boardsRepository;
        private TelemetryClient logger;
        private Stream.StreamClient _streamClient;
		private IdentityHelper _identityHelper;
		private IAppConfiguration _appConfig;

        public ActivityController(
            FeedManagementService feedService,
            FeedItemRepository feedItemRepository,
            CommentsManagementService commentsService,
            CommentsRepository commentsRepository,
            ActivityManagementService activityService,
            ActivityRepository activityRepository,
            JunaUserRepository userRepository,
            ContentUploadService contentUploadService,
            BoardRepository boardsRepository,
            TelemetryClient trace,
            Stream.StreamClient streamClient,
			IdentityHelper identityHelper,
			IAppConfiguration appConfig
            )
        {
            _feedService = feedService;
            _feedItemRepository = feedItemRepository;
            _commentsService = commentsService;
            _commentsRepository = commentsRepository;
            _activityService = activityService;
            _activityRepository = activityRepository;
            _userRepository = userRepository;
            _contentUploadService = contentUploadService;
            _boardsRepository = boardsRepository;
            logger = trace;
            _streamClient = streamClient;
            _identityHelper = identityHelper;
			_appConfig = appConfig;
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get()
        {
            // todo: If unauthenticated, then return all feedItems with continuation token
            // todo: If authenticated, return only feed for the given user
            string continuationToken = Request.Headers[ContinuationTokenHeader].FirstOrDefault();
            var result = _feedItemRepository.QueryAndContinueAsync(continuationToken).Result;
            var feedItems = _feedItemRepository.ConvertToDomainEntities(result.Values.ToArray());

            if (feedItems == null) return new HttpResponseMessage(HttpStatusCode.NotFound);

            continuationToken = result.DbToken;

            var responseMessage = new HttpResponseMessage();
            var stream = new MemoryStream();

            using (var feedItemsStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(feedItems))))
            {
                feedItemsStream.CopyTo(stream);
                responseMessage.Content = new StreamContent(feedItemsStream);
            }

            responseMessage.StatusCode = HttpStatusCode.OK;
            responseMessage.Content.Headers.ContentLength = stream.Length;
            responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            responseMessage.Headers.Add(ContinuationTokenHeader, continuationToken);

            return responseMessage;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var feedItemId = Guid.Parse((string) id);
           
            var feedItem = _feedItemRepository.GetById(feedItemId);

            if (feedItem == null)
                return NotFound();
            else
                return Ok(feedItem);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteById(string id, string boardIdParam)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			var user = _userRepository.GetByObjectId(userId);
			if (user == null) return Unauthorized();

			var feedItemId = Guid.Parse((string) id);
            var boardId = Guid.Parse((string)boardIdParam);

            if (boardId == null)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(userId)) return BadRequest();

            var feedItem = _feedItemRepository.GetById(feedItemId);

            if (feedItem == null) return NotFound();

            var board = _boardsRepository.GetById(boardId);

            if (board == null)
                return NotFound();
            _feedService.DeleteFeedItem(feedItemId, boardId, userId);
            return NoContent();
        }

        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        public IActionResult CreateFeedItems([FromBody] List<FeedItem> data)
        {
            string apiKey = string.Empty;

            if (HttpContext.Request.Headers.TryGetValue("News-Feed-Api-Key", out var tokenNewsFeed))
            {
                apiKey = tokenNewsFeed.FirstOrDefault();
            }

            if (apiKey == _appConfig.AppSettings.NewsFeedApiKey)
            {
                if (data == null) return BadRequest();
                var feedItems = data;

                try
                {
                    if (feedItems != null)
                    {
                        logger.TrackTrace($"Received {feedItems.Count} feedItems", SeverityLevel.Information);
                        feedItems.ForEach(f => ProcessFeedItems(f));
                    }
                }
                catch (Exception ex)
                {
                    // Allow the message to be put into the poison queue
                    logger.TrackTrace($"Error [{ex.Message}] while processing feed items", SeverityLevel.Error);
                    return StatusCode(Status500InternalServerError);
                }

                return StatusCode(Status201Created);
            }

            return StatusCode(Status401Unauthorized);

        }

        [HttpPost]
        [AllowAnonymous]
        [Route("async")]
        public async Task<IActionResult> CreateFeedItemsAsync([FromBody] List<FeedItem> data)
        {
            string apiKey = string.Empty;

            if (HttpContext.Request.Headers.TryGetValue("News-Feed-Api-Key", out var tokenNewsFeed))
            {
                apiKey = tokenNewsFeed.FirstOrDefault();
            }

            if (apiKey == _appConfig.AppSettings.NewsFeedApiKey)
            {
                // make it work then insert to database
                if (data == null)
                    return BadRequest();
                var feedItems = data;

                try
                {
                    if (feedItems != null)
                    {
                        logger.TrackTrace($"Received {feedItems.Count} feedItems", SeverityLevel.Information);

                        foreach (FeedItem feedItem in feedItems)
                        {
                            await ProcessFeedItemsAsync(feedItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Allow the message to be put into the poison queue
                    logger.TrackTrace($"Error [{ex.Message}] while processing feed items", SeverityLevel.Error);
                    return StatusCode(Status500InternalServerError);
                }

                return StatusCode(Status201Created);
            }

            return StatusCode(Status401Unauthorized);

        }

        [HttpPost]
        [Route("{id}/likes")]
        public IActionResult AddLikes(string id, string targetId, string target, string time)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(userId)) return BadRequest();

            // validate all the metadata for likes
            var feedItemId = Guid.Parse((string) id);
            var feedItem = _feedItemRepository.GetById(feedItemId);
            if (feedItem == null) return NotFound();
            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return NotFound();
            _activityService.LikeFeedItem(feedItem, user, target, targetId, time);
            return StatusCode(Status201Created);
        }

        [HttpDelete]
        [Route("{id}/likes")]
        public IActionResult DeleteLikes(string id)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(userId)) return BadRequest();

            var feedItemId = Guid.Parse((string) id);
            var feedItem = _feedItemRepository.GetById(feedItemId);

            if (feedItem == null) return NotFound();

            var user = _userRepository.GetByObjectId(userId);

            if (user == null) return Unauthorized();

             _activityService.UnLike(feedItem, user);
            return NoContent();
        }
        [HttpPost]
        [Route("{id}/disLikes")]
        public IActionResult Dislikes(string id, string target, string targetId, string time)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(id) 
                || string.IsNullOrWhiteSpace(target)
                || string.IsNullOrWhiteSpace(targetId)
                || string.IsNullOrWhiteSpace(time))
                return BadRequest();
            var feedItemId = Guid.Parse((string)id);
            var feedItem = _feedItemRepository.GetById(feedItemId);
            if (feedItem == null) return NotFound();

            var user = _userRepository.GetByObjectId(userId);

            if (user == null) return Unauthorized();
            //if (!_boardsRepository.IsActive(targetId)) return StatusCode(StatusCodes.Status403Forbidden);
            _activityService.DislikeFeedItem(feedItem, user, target, targetId, time);
            return StatusCode(Status201Created);

        }
        [HttpDelete]
        [Route("{id}/disLikes")]
        public IActionResult DeleteDislike(string id)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(userId)) return BadRequest();
            var feedItemId = Guid.Parse((string)id);
            var feedItem = _feedItemRepository.GetById(feedItemId);

            if (feedItem == null) return NotFound();
            var user = _userRepository.GetByObjectId(userId);

            if (user == null) return StatusCode(Status401Unauthorized);
            _activityService.UndoDisLike(feedItem, user);
            return StatusCode(Status204NoContent);
        }

        [HttpPost]
        [Route("{id}/shares")]
        public IActionResult AddShares(string id, string shareTo, string boardId, string time)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (
                string.IsNullOrWhiteSpace(id)
                || string.IsNullOrWhiteSpace(userId)
                || string.IsNullOrWhiteSpace(shareTo)
                || string.IsNullOrWhiteSpace(boardId)
                || string.IsNullOrWhiteSpace(time))

                return BadRequest();

            var feedItemId = Guid.Parse((string) id);
            var feedItem = _feedItemRepository.GetById(feedItemId);

            if (feedItem == null) return NotFound();

            var board = _boardsRepository.GetById(Guid.Parse(boardId));

            if (board == null) return NotFound();

            var user = _userRepository.GetByObjectId(userId);

            if (user == null) return Unauthorized();

            //check board time limit 
            if (BoardValidityHelper.IsBetween(DateTime.Now, board.StartDate.AddHours(-4), board.EndDate.AddHours(4)))
            {
                _activityService.ShareFeedItem(feedItemId, shareTo, Guid.Parse(boardId), userId, time);

                return StatusCode(StatusCodes.Status201Created);
            }
            else if(!BoardValidityHelper.IsBetween(DateTime.Now, board.StartDate.AddHours(-4), board.EndDate.AddHours(4)))
            {
                return Unauthorized();
            }

            return Unauthorized();
        }

        [HttpDelete]
        [Route("{id}/shares")]
        public IActionResult DeleteShares(string id)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(id)
                || string.IsNullOrEmpty(userId)) return BadRequest();

            var feedItemId = Guid.Parse((string) id);
            var feedItem = _feedItemRepository.GetById(feedItemId);

            if (feedItem == null) return NotFound();
            var user = _userRepository.GetByObjectId(userId);

            if (user == null) return Unauthorized();
            _activityService.UnshareFeedItem(feedItemId, userId);
            return NoContent();
        }

        [HttpPost]
        [Route("AddEmoji")]
        public IActionResult AddEmoji(string id, string boardId, string emojiId)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(emojiId))
                return BadRequest();

            if (boardId != null)
            {
                var board = _boardsRepository.GetById(Guid.Parse(boardId));

                if (board != null)
                {
                    if (BoardValidityHelper.IsBetween(DateTime.Now, board.StartDate.AddHours(-4), board.EndDate.AddHours(4)))
                    {
                        if (!_boardsRepository.IsActive(boardId))
                            return StatusCode(StatusCodes.Status403Forbidden);
                    }
                    else if (!BoardValidityHelper.IsBetween(DateTime.Now, board.StartDate.AddHours(-4), board.EndDate.AddHours(4)))
                    {
                        return Unauthorized();
                    }
                }
            }

            return StatusCode(StatusCodes.Status403Forbidden);
        }

        [HttpPost]
        [Route("{id}/comments")]
        public IActionResult AddComments(string id, string boardId ,[FromBody] string comment, string time)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			// TODO:   Add Date validation. For now storing it as string.
			var timeStamp = time;
            if (
                string.IsNullOrWhiteSpace(id)
                || string.IsNullOrWhiteSpace(userId)
                || string.IsNullOrWhiteSpace(boardId)
                || string.IsNullOrWhiteSpace(comment)
                || string.IsNullOrWhiteSpace(timeStamp)) return BadRequest();

            var feedItemId = Guid.Parse((string) id);
            var feedItem = _feedItemRepository.GetById(feedItemId);
            var board = _boardsRepository.GetById(Guid.Parse(boardId));
            if (feedItem == null || board == null) return NotFound();

            var user = _userRepository.GetByObjectId(userId);

            if (user == null)
                return Unauthorized();

            if (BoardValidityHelper.IsBetween(DateTime.Now, board.StartDate.AddHours(-4), board.EndDate.AddHours(4)))
            {
                _commentsService.StoreComments(board, feedItem, user, timeStamp, comment);

                return StatusCode(Status201Created);
            }
            else if (!BoardValidityHelper.IsBetween(DateTime.Now, board.StartDate.AddHours(-4), board.EndDate.AddHours(4)))
            {
                return Unauthorized();
            }

            return Unauthorized();
        }

        [HttpGet("{id}/comments")]
        public IActionResult GetComments(string id)
        {
            var feedItemId = Guid.Parse((string)id);
            FeedItem feedItem = _feedItemRepository.GetById(feedItemId);
            if (feedItem == null)
                return StatusCode(Status404NotFound);
            feedItem = _commentsService.GetCommentsByFeedItem(feedItemId);
            if (feedItem != null)
                return Ok(feedItem);
            return StatusCode(Status404NotFound);
        }
        
        [HttpDelete]
        [Route("{id}/comments/{commentId}")]
        public IActionResult DeleteComments(string id, string commentId)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(id)
                || string.IsNullOrEmpty(userId)
                || string.IsNullOrEmpty(commentId))
                return BadRequest();

            var feedItemId = Guid.Parse((string) id);
            var feedItem = _feedItemRepository.GetById(feedItemId);
            if (feedItem == null) return NotFound();
            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();
            var activity = _commentsRepository.GetById(Guid.Parse(commentId));
            if (activity == null) return StatusCode(Status404NotFound);
            else
            {
                _commentsService.DeleteComments(feedItem, activity);
                return StatusCode(Status204NoContent);
            }
        }

        [HttpPost]
        [Route("{id}/comments/{replyToId}")]
        public IActionResult AddReply(string id, string boardId, string replyToId, [FromBody] string replyComment, string time)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
            var board = new Board();

            if(boardId != null)
            {
                board = _boardsRepository.GetById(Guid.Parse(boardId));
             
                if (BoardValidityHelper.IsBetween(DateTime.Now, board.StartDate.AddHours(-4), board.EndDate.AddHours(4)))
                {
                    //enable board
                    // enable board
                    var timeStamp = time;
                    if (
                        string.IsNullOrWhiteSpace(id)
                        || string.IsNullOrWhiteSpace(userId)
                        || string.IsNullOrWhiteSpace(replyComment)
                        || string.IsNullOrWhiteSpace(timeStamp)
                        || string.IsNullOrWhiteSpace(replyToId))

                        return BadRequest();

                    var feedItemId = Guid.Parse((string)id);
                    var parentCommentId = Guid.Parse((string)replyToId);
                    var feedItem = _feedItemRepository.GetById(feedItemId);
                    board = _boardsRepository.GetById(Guid.Parse(boardId));
                    var parentComment = _commentsRepository.GetById(parentCommentId);

                    if (feedItem == null || parentComment == null) return NotFound();

                    var user = _userRepository.GetByObjectId(userId);

                    if (user == null) return Unauthorized();
                    _commentsService.ReplyToComment(board, feedItem, user, timeStamp, replyComment, parentComment);

                    return StatusCode(Status201Created);
                }
                else if (!BoardValidityHelper.IsBetween(DateTime.Now, board.StartDate.AddHours(-4), board.EndDate.AddHours(4)))
                {
                    //disable board
                    return Unauthorized();
                }
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("{id}/pins")]
        public IActionResult AddPins(string id, string target, string targetId, string time)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (
                string.IsNullOrWhiteSpace(id)
                || string.IsNullOrWhiteSpace(userId)
                || string.IsNullOrWhiteSpace(target)
                || string.IsNullOrWhiteSpace(targetId)
                || string.IsNullOrWhiteSpace(time)) return BadRequest();

            // validate all the metadata for pins
            var feedItemId = Guid.Parse((string)id);
            var feedItem = _feedItemRepository.GetById(feedItemId);
            if (feedItem == null) return NotFound();

            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();
            _activityService.PinFeedItem(feedItem, user, target, targetId, time);
            return StatusCode(Status201Created);
        }

        [HttpDelete]
        [Route("{id}/pins/{pinId}")]
        public IActionResult DeletePins(string id, string pinId)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(id)
                || string.IsNullOrEmpty(userId)
                || string.IsNullOrEmpty(pinId))
                return BadRequest();

            var feedItemId = Guid.Parse((string) id);
            var feedItem = _feedItemRepository.GetById(feedItemId);

            if (feedItem == null) return NotFound();

            var user = _userRepository.GetByObjectId(userId);

            if (user == null) return Unauthorized();

            var activity = _activityRepository.GetById(Guid.Parse(pinId));
            if (activity == null) return StatusCode(StatusCodes.Status404NotFound);
            else
            {
                _activityService.DeletePin(feedItem, pinId);
                return StatusCode(Status204NoContent);
            }
        }
        
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload(string targetId, string feedType,
            string contentType, string dateCreated, string description, [FromForm]List<IFormFile> files)
        {
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			var targetIdGuid = Guid.Parse(targetId);
            var timeStamp = (string)dateCreated;
            if (targetId == null
               || string.IsNullOrWhiteSpace(feedType)
               || string.IsNullOrWhiteSpace(contentType)
               || string.IsNullOrWhiteSpace(timeStamp)
               )
                return BadRequest();
            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();
            var postedFile = Request.Form.Files.FirstOrDefault();
            if (postedFile == null) return BadRequest();
            var queryInfo = Request.Form;

            FeedItem fileInfo = new FeedItem(FeedItem.ImageFeedItem)
            {
                Id = Guid.NewGuid(),
                // todo: This will throw exception for bad timestamps. Add code to validate this
                DateCreated = DateTime.Parse(timeStamp),
            };
            switch (feedType)
            {
                case FeedGroup.BoardFeedType:
                    var board = _boardsRepository.GetById(targetIdGuid);
                    if (board == null) return NotFound();
                    fileInfo.Title = postedFile.Name;
                    await _contentUploadService
                        .UploadFile(fileInfo, user, postedFile.ContentType,
                        postedFile.OpenReadStream(), feedType, board, description);
                    break;
                case FeedGroup.UserFeedType:
                    fileInfo.Title = postedFile.Name;
                    try
                    {
                        await _contentUploadService.UploadFile(fileInfo, user, postedFile.ContentType,
                            postedFile.OpenReadStream(), feedType, description);
                        break;
                    }
                    catch (Exception e)
                    {
                        return StatusCode(Status500InternalServerError);
                    }
                default:
                    return BadRequest();
            }
            return StatusCode(Status201Created);
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

                feedItem = _feedService.StoreItemWithUniqueUrl(feedItem);
                // Send Post Activity To Stream
               var  activity = _activityService.StoreUniqueActivity(new Activity
                {
                    Actor = ActivityHelper.GetActor(FOOTBALL_BOT),
                    Verb = InteractionMetadata.INTERACTION_POST,
                    Object = ActivityHelper.GetObject(feedItem),
                    Time = JsonConvert.SerializeObject(feedItem.DatePublished)
                });

                var junaUserFeed = _streamClient.Feed(FeedGroup.JunaFeedType, StreamHelper.GetStreamActorId(activity));
                var streamActivity = new Stream.Activity(activity.Actor, activity.Verb, activity.Object)
                {
                    ForeignId = activity.Id.ToString(),
                };
                junaUserFeed.AddActivity(streamActivity);
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

        private async Task ProcessFeedItemsAsync(FeedItem feedItem)
        {
            try
            {
                logger.TrackTrace($"Processing feed item with Title [{feedItem.Title}] of type: [{feedItem.ContentType}]", SeverityLevel.Information);
                logger.TrackTrace("\nInserting newsfeed into the database\n", SeverityLevel.Information);

                logger.TrackTrace($"\n New URL is [{feedItem.Title}]", SeverityLevel.Information);
                logger.TrackTrace(JsonConvert.SerializeObject(feedItem), SeverityLevel.Verbose);
                
                feedItem = await _feedService.StoreItemWithUniqueUrlAsync(feedItem);

                var activity = _activityService.StoreUniqueActivity(new Activity
                {
                    Actor = ActivityHelper.GetActor(FOOTBALL_BOT),
                    Verb = InteractionMetadata.INTERACTION_POST,
                    Object = ActivityHelper.GetObject(feedItem),
                    Time = JsonConvert.SerializeObject(feedItem.DatePublished)
                });

                var junaUserFeed = _streamClient.Feed(FeedGroup.JunaFeedType, StreamHelper.GetStreamActorId(activity));
                var streamActivity = new Stream.Activity(activity.Actor, activity.Verb, activity.Object)
                {
                    ForeignId = activity.Id.ToString(),
                };
                await junaUserFeed.AddActivity(streamActivity);



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