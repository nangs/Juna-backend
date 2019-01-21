using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.ApplicationInsights;
using Juna.Feed.Service;
using Juna.Feed.Repository;
using System.Security.Claims;
using Juna.Feed.WebApi.Helpers;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Juna.Feed.WebApi.Controllers
{
    [Authorize]
    [Route("moderations")]
    [ApiController]
    public class ModerationsController : ControllerBase
    {
		private FeedItemRepository _feedItemRepository;
        private CommentsRepository _commentsRepository;
        private JunaUserRepository _userRepository;
        private BoardRepository _boardsRepository;
        private ModerationManagementService _moderationManagementService;
		private IdentityHelper _identityHelper;
        private readonly TelemetryClient logger;

        public ModerationsController(
            CommentsRepository commentsRepository,
			FeedItemRepository feedItemRepository,
            JunaUserRepository userRepository,
            ModerationManagementService moderationManagementService,
            BoardRepository boardsRepository,
            TelemetryClient trace,
			IdentityHelper identityHelper
            )
          {
            _commentsRepository = commentsRepository;
			_feedItemRepository = feedItemRepository;
            _userRepository = userRepository;
            _boardsRepository = boardsRepository;
            _moderationManagementService = moderationManagementService;
			_identityHelper = identityHelper;
            logger = trace;
        }

        [HttpPost]
        [Route("blocks")]
        public IActionResult BlockUser(string blockUserId, string time)
        {
			// APi To Block
			// TODO : Retreive provider and UserId Of User who is to be blocked
			//        write functionality to Block the user
			//        send bad request if not found
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(blockUserId)
            || string.IsNullOrWhiteSpace(userId)
            || string.IsNullOrWhiteSpace(time)) return BadRequest();

            var user = _userRepository.GetByObjectId(userId);
            var blockUser = _userRepository.GetByObjectId(blockUserId);

            if (user == null)
                return Unauthorized();

            if (blockUser == null)
                return NotFound();
            _moderationManagementService.BlockUser(user, blockUser, time);
            return StatusCode(Status201Created);
        }

        [HttpDelete]
        [Route("blocks")]
        public IActionResult UnBlockUser(string blockUserId)
        {
			// APi To UnBlock
			// TODO : Retreive provider and UserId Of User who is to be Unblocked
			//        write functionality to UnBlock the user
			//        send bad request if not found

			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(blockUserId)
            || string.IsNullOrWhiteSpace(userId)) return BadRequest();

            var user = _userRepository.GetByObjectId(userId);
            var blockUser = _userRepository.GetByObjectId(blockUserId);

            if (user == null)
                return Unauthorized();

            if (blockUser == null)
                return NotFound();
            try
            {
                _moderationManagementService.UnBlock(user, blockUser);
            }
            catch (Exception)
            {
                return StatusCode(Status500InternalServerError);
            }
            return NoContent();
        }

        [HttpPost]
        [Route("bans")]
        public IActionResult BanUser(string banUserId, string boardId, string time)
        {
			// APi To Ban
			// TODO : Retreive provider , UserId and boardId Of User who is to be banned
			//        write functionality to Ban the user
			//        send bad request if not found
			// TODO: Only users with moderator or higher privilege should be able to ban a user

			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(banUserId)
            || string.IsNullOrWhiteSpace(userId)
            || string.IsNullOrWhiteSpace(boardId)
            || string.IsNullOrWhiteSpace(time)) return BadRequest();

            var user = _userRepository.GetByObjectId(userId);
            var banUser = _userRepository.GetByObjectId(banUserId);
            var board = _boardsRepository.GetById(Guid.Parse(boardId));

            if (user == null)
                return Unauthorized();

            if (banUser == null || board == null)
                return NotFound();
            _moderationManagementService.BanUser(user, banUser, board, time);
            return StatusCode(Status201Created);
        }

        [HttpDelete]
        [Route("bans")]
        public IActionResult UnBanUser(string banUserId, string boardId)
        {
			// APi To Remove Ban
			// TODO : Retreive provider , UserId and boardId Of User whose ban is to be removed
			//        write functionality to remove the ban
			//        send bad request if not found

			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(banUserId)
            || string.IsNullOrWhiteSpace(userId)
            || string.IsNullOrWhiteSpace(boardId))
                return BadRequest();

            var user = _userRepository.GetByObjectId(userId);
            var bannedUser = _userRepository.GetByObjectId(banUserId);
            var board = _boardsRepository.GetById(Guid.Parse(boardId));

            if (user == null) return Unauthorized();

            if (bannedUser == null || board == null) return NotFound();

            try
            {
                _moderationManagementService.UnBanUser(board, user, bannedUser);
            }
            catch (Exception)
            {
                return StatusCode(Status500InternalServerError);
            }
            return NoContent();

        }

        [HttpPost]
        [Route("mutes")]
        public IActionResult MuteUser(string muteUserId, string boardId, string time)
        {
			// APi To mute
			// TODO : Retreive provider , UserId and boardId Of User who is to be muted
			//        write functionality to mute the user
			//        send bad request if not found

			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

			if (string.IsNullOrWhiteSpace(muteUserId)
            || string.IsNullOrWhiteSpace(userId)
            || string.IsNullOrWhiteSpace(boardId)
            || string.IsNullOrWhiteSpace(time))
                return BadRequest();

            var user = _userRepository.GetByObjectId(userId);
            var muteUser = _userRepository.GetByObjectId(muteUserId);
            var board = _boardsRepository.GetById(Guid.Parse(boardId));

            if (user == null) return Unauthorized();

            if (muteUser == null || board == null) return NotFound();
            _moderationManagementService.MuteUser(user, muteUser, board, time);
            return StatusCode(Status201Created);
        }

        [HttpDelete]
        [Route("mutes")]
        public IActionResult UnMuteUser(string muteUserId, string boardId)
        {
			// APi To Unmute
			// TODO : Retreive provider , UserId and boardId Of User who is to be Unmuted
			//        write functionality to Unmute the user
			//        send bad request if not found
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
			if (string.IsNullOrWhiteSpace(muteUserId)
            || string.IsNullOrWhiteSpace(userId)
            || string.IsNullOrWhiteSpace(boardId))
                return BadRequest();

            var user = _userRepository.GetByObjectId(userId);
            var mutedUser = _userRepository.GetByObjectId(muteUserId);
            var board = _boardsRepository.GetById(Guid.Parse(boardId));

            if (user == null) return Unauthorized();

            if (mutedUser == null || board == null) return NotFound();
            try
            {
                _moderationManagementService.UnMuteUser(board, user, mutedUser);
            }
            catch (Exception)
            {
                return StatusCode(Status500InternalServerError);
            }
            return NoContent();
        }

        [HttpPost]
        [Route("TimeOuts")]
        public IActionResult TimeOuts(string provider, string boardId, string duration)
        {
			// APi To TimeOuts
			// TODO : Retreive provider , UserId ,boardId and duration Of User to be subjected to TimeOut
			//        Disable Certain Features of user for timeout duration
			//        send bad request if not found
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
			if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(boardId) || string.IsNullOrEmpty(duration))
                return BadRequest();
            else
                return StatusCode(Status201Created);
        }

        [HttpPost]
        [Route("reports")]
        public IActionResult Reports(string objectId, string boardId, string time)
        {
			// APi For Reporting
			// TODO :  Retreive the data to be reported
			//         Report the data     
			var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
			if (string.IsNullOrWhiteSpace(objectId)
            || string.IsNullOrWhiteSpace(userId)
            || string.IsNullOrWhiteSpace(boardId))
                return BadRequest();

            var user = _userRepository.GetByObjectId(userId);

            if (user == null) return Unauthorized();

            var board = _boardsRepository.GetById(Guid.Parse(boardId));

            if (board == null) return NotFound();

            var reportUser = _userRepository.GetByObjectId(objectId);
            var comment = _commentsRepository.GetById(Guid.Parse(objectId));
            var reportFeedItem = _feedItemRepository.GetById(Guid.Parse(objectId));
            _moderationManagementService.Report(reportUser, comment, reportFeedItem, user, board, time);
            return StatusCode(Status201Created);
        }
    }
}