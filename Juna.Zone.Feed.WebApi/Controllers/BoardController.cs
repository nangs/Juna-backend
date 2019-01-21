using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Juna.Feed.DomainModel;
using Juna.Feed.Repository;
using Juna.Feed.Service;
using Juna.FeedFlows.DomainModel.Service;
using Microsoft.ApplicationInsights;
using System.Security.Claims;
using Juna.Feed.WebApi.Helpers;
using Juna.Feed.Service.Helpers;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Juna.Feed.DomainModel.Builder;

namespace Juna.Feed.WebApi.Controllers
{
    [Authorize]
    [Route("boards")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        private static readonly JunaUser FOOTBALL_BOT_USER = new JunaUser
        {
            Id = Guid.NewGuid(),
            ObjectId = Guid.NewGuid().ToString(),
            DisplayName = FeedManagementService.FOOTBALL_BOT
        };

        private FeedManagementService _feedService;
        private readonly FeedItemRepository _feedItemRepository;
        private BoardManagementService _boardManagementService;
        private BoardRepository _boardsRepository;
        private readonly ActivityManagementService _activityService;
        private readonly ActivityRepository _activityRepository;
        private JunaUserRepository _userRepository;
        private readonly ContentUploadService _contentUploadService;
        private readonly BlobHelper _blobHelper;
        private readonly TelemetryClient logger;
        private readonly IAppConfiguration appConfig;
        private IdentityHelper _identityHelper;
        private FCMSenderService _fcmSenderService;

        public BoardController(
             FeedManagementService feedService,
             FeedItemRepository feedItemRepository,
             BoardManagementService boardmanagementservice,
             BoardRepository boardsRepository,
             ActivityManagementService activityService,
             ActivityRepository activityRepository,
             JunaUserRepository userRepository,
             ContentUploadService contentUploadService,
             BlobHelper blobHelper,
             TelemetryClient trace,
             IAppConfiguration appConfiguration,
             IdentityHelper identityHelper,
             FCMSenderService fcmSenderService

            )
        {
            _feedService = feedService;
            _feedItemRepository = feedItemRepository;
            _boardManagementService = boardmanagementservice;
            _boardsRepository = boardsRepository;
            _activityService = activityService;
            _activityRepository = activityRepository;
            _userRepository = userRepository;
            _contentUploadService = contentUploadService;
            _blobHelper = blobHelper;
            logger = trace;
            appConfig = appConfiguration;
            _identityHelper = identityHelper;
            _fcmSenderService = fcmSenderService;
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var board = _boardManagementService.GetBoard(id);

            if (board == null)
                return NotFound();
            else
                return Ok(board);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetByBoardEvent(long foreignId, string boardType)
        {
            if (!_identityHelper.IsAuthenticated(User.Identity as ClaimsIdentity))
            {
                var apiKey = string.Empty;

                if (HttpContext.Request.Headers.TryGetValue("Football-Data-Api-Key", out var tokenFootballData))
                {
                    apiKey = tokenFootballData.ToString().Split(',')[0];
                }
                if (apiKey != appConfig.AppSettings.FootballDataApiKey) return Unauthorized();
            }

            if (string.IsNullOrEmpty(boardType) || !BoardEvent.EventTypes.Contains(boardType)) return BadRequest();

            var board = _boardManagementService.GetByBoardEvent(foreignId, boardType);
            if (board == null)
                return NotFound();
            else
                return Ok(board);
        }

        [HttpGet("myBoards")]
        public IActionResult RetrieveBoardsThatUserIsPartOf()
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();
            var boards = _boardManagementService.GetBoardsThatUserIsPartOf(user);
            if (boards == null)
                return NotFound();
            else
                return Ok(boards);
        }

        [HttpGet("{id}/members")]
        public IActionResult RetrieveAllMembersOfBoard(string id)
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();
            var board = _boardsRepository.GetById(Guid.Parse(id));
            if (board == null) return NotFound();
            var members = _boardManagementService.GetAllMembersOfBoard(user, board);
            if (members == null)
                return NotFound();
            else
                return Ok(members);
        }

        [HttpGet("feedItems")]
        public IActionResult GetFeedItems(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var boardId = Guid.Parse((string)id);
            var feedItems = _feedService.GetFeedItems(boardId);

            if (feedItems == null || feedItems.Count == 0) return StatusCode(Status404NotFound);
            return Ok(feedItems);
        }

        [HttpPost]
        [Route("{id}/feedItems")]
        public IActionResult PostFeedItem(string id, string contentType, [FromBody] string title, string dateCreated)
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

            if (string.IsNullOrWhiteSpace(id)
                || string.IsNullOrWhiteSpace(userId)
                || string.IsNullOrWhiteSpace(contentType)
                || string.IsNullOrWhiteSpace(title)
                || string.IsNullOrWhiteSpace(dateCreated)) return StatusCode(StatusCodes.Status400BadRequest);

            var boardId = Guid.Parse((string)id);
            var board = _boardsRepository.GetById(boardId);
            if (board == null) return NotFound();
            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();
            _feedService.CreateFeedItem(board, user, contentType, title, DateTime.Parse(dateCreated));
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Post(long footballMatchId, string eventType, string homeTeam,
            string visitorTeam, string matchStartTime, [FromBody]  Board privateBoardData, string boardType)
        {
            switch (boardType)
            {
                case ("public"):
                    {
                        var apiKey = string.Empty;

                        if (HttpContext.Request.Headers.TryGetValue("Football-Data-Api-Key", out var tokenFootballData))
                        {
                            apiKey = tokenFootballData.ToString().Split(',')[0];
                        }
                        if (apiKey != appConfig.AppSettings.FootballDataApiKey) return Unauthorized();
                        if (footballMatchId <= 0
                        || string.IsNullOrWhiteSpace(eventType)
                        || string.IsNullOrWhiteSpace(homeTeam)
                        || string.IsNullOrWhiteSpace(visitorTeam)
                        || string.IsNullOrWhiteSpace(matchStartTime)) throw new InvalidOperationException();

                        _boardManagementService.CreateBoard(new Board()
                        {
                            Id = Guid.NewGuid(),
                            Displayname = ($"{homeTeam} vs {visitorTeam}"),
                            BoardType = ($"{eventType}"),
                            StartDate = DateTime.Parse(matchStartTime).AddHours(-4), // board goes active 4 hrs before match start 
                            EndDate = DateTime.Parse(matchStartTime).AddHours(6), // board goes inactive after 4 hrs of match start
                            BoardEvent = new BoardEvent
                            {
                                Type = $"{eventType}",
                                ForeignId = footballMatchId
                            }
                        });
                        break;
                    }

                case ("private"):
                    {
                        var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
                        var user = _userRepository.GetByObjectId(userId);
                        if (user == null) return Unauthorized();

                        if (privateBoardData == null)
                            return StatusCode(StatusCodes.Status400BadRequest);
                        var board = _boardManagementService.CreatePrivateBoard(new Board
                        {
                            Id = Guid.NewGuid(),
                            Name = privateBoardData.Name,
                            Owner = user,
                            CreatedBy = user,
                            Zone = privateBoardData.Zone,
                            Color = privateBoardData.Color,
                            Description = privateBoardData.Description,
                            CreateTime = DateTime.UtcNow.ToString(),
                            BoardType = boardType,
                            Displayname = privateBoardData.Displayname
                        });
                        return Content(Newtonsoft.Json.JsonConvert.SerializeObject(board.Id), "text/plain");
                    }

                default:
                    return StatusCode(StatusCodes.Status400BadRequest);
            }
            // TODO: Change Return Value To Board
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        [Route("{id}/invite")]
        public async Task<IActionResult> InviteToBoardAsync(string id, [FromBody] List<InviteeUserIdList> inviteeUserIdList)
        {
            if (string.IsNullOrWhiteSpace(id)
               || inviteeUserIdList == null) return BadRequest();

            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
            var user = _userRepository.GetByObjectId(userId);

            var boardId = Guid.Parse((string)id);
            var board = _boardsRepository.GetById(boardId);
            if (board == null) return NotFound();

            foreach (var inviteeUserId in inviteeUserIdList)
            {
                var inviteeUser = _userRepository.GetByObjectId(inviteeUserId.objectId);
                if (user == null) return Unauthorized();
                if (inviteeUser == null) return NotFound();
                var activity = new ActivityBuilder()
                                        .WithActor(user)
                                        .WithVerb(BoardInteractionMetadata.BOARD_INTERACTION_INVITE)
                                        .WithObject(inviteeUser)
                                        .Build();
                _activityRepository.Save(activity);
                await _fcmSenderService.SendBoardInviteNotification(
                  new BoardInviteNotification
                  {
                      BoardId = board.Id,
                      UserId = user.ObjectId,
                      InviterName = user.DisplayName,
                      InviteeUserId = inviteeUser.ObjectId,
                      InvitationLink = $"{appConfig.AppSettings.JunaRestApiEndpoint}/boards/{board.Id}/activities/follow?id={board.Id}"
                  },
                  FCMSenderService.CREATE_OPERATION);
            }
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        [Route("{id}/activities/enter")]
        public IActionResult UserEntersBoard(string id, string boardType)
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(userId)) throw new InvalidOperationException();

            var boardId = Guid.Parse((string)id);
            var board = _boardsRepository.GetById(boardId);

            if (board == null) return NotFound();

            var user = _userRepository.GetByObjectId(userId);

            if (user == null) return Unauthorized();
            switch (boardType)
            {
                case ("public"):
                    _boardManagementService.UserEntersBoard(
                        user,
                        DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                        board);
                    break;
                case ("private"):
                    var activity = _activityRepository.GetByVerbAndObject(BoardInteractionMetadata.BOARD_INTERACTION_INVITE, user.ObjectId);
                    if (activity != null)
                        _boardManagementService.UserEntersBoard(
                   user,
                   DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                   board);
                    else
                        return Unauthorized();
                    break;
                default:
                    return BadRequest();
            }
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpDelete]
        [Route("{id}/activities/exit")]
        public IActionResult ExitActivities(string id)
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

            var boardId = Guid.Parse((string)id);
            var board = _boardsRepository.GetById(boardId);

            if (board == null) return NotFound();

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(userId)) throw new InvalidOperationException();

            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();

            _boardManagementService.UserExitBoard(
                    user,
                    DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                    board
                    );
            _boardsRepository.Upsert(board);
            return NoContent();
        }

        [HttpPost]
        [Route("{id}/activities/follow")]
        public IActionResult FollowActivities(string id)
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(userId)) throw new InvalidOperationException();

            var boardId = Guid.Parse((string)id);
            var board = _boardsRepository.GetById(boardId);
            if (board == null) return NotFound();

            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();

            _boardManagementService.UserFollowsBoard(
                user,
                DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                board);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpDelete]
        [Route("{id}/activities/unfollow")]
        public IActionResult UnFollowActivities(string id)
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

            if (string.IsNullOrWhiteSpace(id)
                || string.IsNullOrWhiteSpace(userId)) throw new InvalidOperationException();

            var boardId = Guid.Parse((string)id);
            var board = _boardsRepository.GetById(boardId);
            if (board == null) return NotFound();

            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();

            _boardManagementService.UserUnFollowsBoard(
                user,
                DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                board);
            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("live")]
        public async Task<IActionResult> LiveData([FromBody] LiveEvent liveEvent)
        {
            string apiKey = string.Empty;

            if (HttpContext.Request.Headers.TryGetValue("Football-Data-Api-Key", out var tokenFootballData))
            {
                apiKey = tokenFootballData.ToString().Split(',')[0];
            }

            if (HttpContext.Request.Headers.TryGetValue("Activation-Notifier-Api-Key", out var tokenActivationNotifier))
            {
                apiKey = tokenActivationNotifier.ToString().Split(',')[0];
            }

            if (apiKey == appConfig.AppSettings.FootballDataApiKey)
            {
                await _fcmSenderService.SendBoardLiveData(liveEvent);
                return StatusCode(StatusCodes.Status200OK);
            }
            else
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
        }


    }

    public class InviteeUserIdList
    {
        public string objectId;
    }
}