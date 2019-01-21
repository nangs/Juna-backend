using Juna.Feed.DomainModel;
using Juna.Feed.Repository;
using Juna.Feed.Service.Helpers;
using Juna.Feed.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Juna.Feed.WebApi.Controllers
{
    [Authorize]
    [Route("feedEntries")]
    [ApiController]
    public class FeedEntryController : ControllerBase
    {
        private const string FOOTBALL_BOT = "JunaFootball";
        private FeedItemRepository _feedItemRepository;
        private ActivityRepository _activityRepository;
        private readonly JunaUserRepository _userRepository;
        private Stream.StreamClient _streamClient;
		private IdentityHelper _identityHelper;
        public FeedEntryController(FeedItemRepository feedItemRepository,
            ActivityRepository activityRepository,
            JunaUserRepository userRepository,
            Stream.StreamClient streamClient,
			IdentityHelper identityHelper)
        {
            _feedItemRepository = feedItemRepository;
            _activityRepository = activityRepository;
            _userRepository = userRepository;
            _streamClient = streamClient;
			_identityHelper = identityHelper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<List<FeedEntry>> GetAsync()
        {
            var feedEntryList = new List<FeedEntry>();
            var identity = User.Identity as ClaimsIdentity;
            if (!identity.IsAuthenticated)
            {
                // This is an anoymous user
                var anonymousUserFeed = _streamClient.Feed(FeedGroup.JunaFeedType, FOOTBALL_BOT);
                var streamActivities = await anonymousUserFeed.GetActivities(0, 30);
                foreach (var activity in streamActivities)
                {
                    var feedItemId = StreamHelper.GetStreamObjectId(activity);
                    var feedItem = _feedItemRepository.GetById(Guid.Parse(feedItemId));
					feedEntryList.Add(new FeedEntry
					{
						FeedItem = feedItem,
						Activity = new DomainModel.Activity
						{
							Id = Guid.Parse(activity.Id),
							Actor = activity.Actor,
							Verb = activity.Verb,
							Object = activity.Object,
							Target = activity.Target,
							ForeignId = activity.ForeignId,
							Time = activity.Time.ToString()
						}
					});
                }
            }
            else
            {
				var userObjectID = _identityHelper.GetObjectId(identity);
                var registeredUserFeed = _streamClient.Feed(FeedGroup.UserFeedType, userObjectID);
                var streamActivities = await registeredUserFeed.GetActivities(0, 30);
                foreach (var activity in streamActivities)
                {
                    var feedItemId = StreamHelper.GetStreamObjectId(activity);

                    feedEntryList.Add(new FeedEntry
					{
						FeedItem = _feedItemRepository.GetById(Guid.Parse(feedItemId)),
						Activity = _activityRepository.GetById(Guid.Parse(activity.ForeignId))
					});
                }
            }

            return feedEntryList;
        }
    }
}
