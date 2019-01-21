using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.ApplicationInsights;
using Juna.Feed.DomainModel;
using Juna.Feed.Service;
using System.Security.Claims;
using Juna.Feed.WebApi.Helpers;
using Juna.Feed.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juna.Feed.WebApi.Controllers
{
    [Authorize]
    [Route("users")]
    [ApiController]
    public class JunaUserController : ControllerBase
    {
        private ActivityRepository _activityRepository;
        private JunaUserService _junaUserService;
        private readonly TelemetryClient _logger;
        private IdentityHelper _identityHelper;
        private JunaUserRepository _userRepository;

        public JunaUserController(ActivityRepository activityRepository, JunaUserService junaUserService, TelemetryClient logger,
            JunaUserRepository userRepository,
            IdentityHelper identityHelper)
        {
            _activityRepository = activityRepository;
            _junaUserService = junaUserService;
			_identityHelper = identityHelper;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet("{email}")]
        public ActionResult<JunaUser> GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return NotFound();

            //find user in azure AD B2C
            var azureUserEmail = User.FindFirst("emails")?.Value;

            if (azureUserEmail == null || !email.Equals(azureUserEmail)) return Forbid();

            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
            var user = _junaUserService.GetJunaUserByEmail(email: email, objectId: userId);
            if (user == null) return NotFound();
            return user;
        }

        [HttpGet("{displayName}")]
        public ActionResult<List<JunaUser>> GetUserByDisplayName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return NotFound();
            var users = _userRepository.GetByDisplayName(displayName);
            if (users == null) return NotFound();
            return users;
        }

        [HttpGet("")]
        public ActionResult<JunaUser> GetUser()
        {
            //find user in azure AD B2C
            var azureUserEmail = User.FindFirst("emails")?.Value;
            var azureUserDisplayName = User.FindFirst("name")?.Value;
            var azureUserCountry = User.FindFirst("country")?.Value;
            var azureUserIdentityProvider = User.FindFirst("identityProvider")?.Value;
            var azureUserCity = User.FindFirst("city")?.Value;
            var azureUserGivenName = User.FindFirst("givenName")?.Value;
            var azureUserPostalCode = User.FindFirst("postalCode")?.Value;
            var azureUserJobTitle = User.FindFirst("jobTitle")?.Value;
            var azureUserStreetAddress = User.FindFirst("streetAddress")?.Value;
            var azureUserSurName = User.FindFirst("surname")?.Value;

			var objectId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

            return _junaUserService.CreateNewOrGetExistingUser(
                    email: azureUserEmail,
                    displayName: azureUserDisplayName,
                    country: azureUserCountry,
                    identityProvider: azureUserIdentityProvider,
                    city: azureUserCity,
                    givenName: azureUserGivenName,
                    postalCode: azureUserPostalCode,
                    jobTitle: azureUserJobTitle,
                    streetAddress: azureUserStreetAddress,
                    surname: azureUserSurName,
                    userObjectID: objectId);
        }



        [HttpPost]
        [Route("follow")]
        public IActionResult FollowUser(string followUserId)
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
            var time = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(followUserId)
            || string.IsNullOrWhiteSpace(userId)) return BadRequest();

            var user = _userRepository.GetByObjectId(userId);
            var followUser = _userRepository.GetByObjectId(followUserId);

            if (user == null)
                return Unauthorized();

            if (followUser == null)
                return NotFound();
            _junaUserService.FollowUser(user, followUser, time);

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpDelete]
        [Route("unFollow")]
        public IActionResult UnFollowUser(string followUserId)
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);

            if (string.IsNullOrWhiteSpace(followUserId)
            || string.IsNullOrWhiteSpace(userId)) return BadRequest();

            var user = _userRepository.GetByObjectId(userId);
            var followUser = _userRepository.GetByObjectId(followUserId);

            if (user == null)
                return Unauthorized();

            if (followUser == null)
                return NotFound();
            _junaUserService.UnFollowUser(user, followUser);
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpGet("followers")]
        public ActionResult<List<JunaUser>> GetFollowers()
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();
            var objectIds = _activityRepository.GetByVerbAndObject(
                                    verb: InteractionMetadata.INTERACTION_FOLLOW,
                                    objectString: $"JunaUser:{user.ObjectId}")
                .ToList().Select(a => a.Actor.Split(':').Skip(1).FirstOrDefault()).ToList();
            var followers = _userRepository.GetByObjectIds(objectIds);
            if (followers == null || followers.Count == 0)
                return NotFound();
            else
                return followers;
        }


        [HttpGet("following")]
        public ActionResult<List<JunaUser>> GetUsersWhomUserIsFollowing()
        {
            var userId = _identityHelper.GetObjectId(User.Identity as ClaimsIdentity);
            var user = _userRepository.GetByObjectId(userId);
            if (user == null) return Unauthorized();
            var objectIds = _activityRepository.GetByVerbAndActor(
                                    verb: InteractionMetadata.INTERACTION_FOLLOW,
                                    actor: $"JunaUser:{user.ObjectId}")
                .ToList().Select(a => a.Object.Split(':').Skip(1).FirstOrDefault()).ToList();
            var followeringUsers = _userRepository.GetByObjectIds(objectIds);
            if (followeringUsers == null || followeringUsers.Count == 0)
                return NotFound();
            else
                return followeringUsers;
        }




    }
}