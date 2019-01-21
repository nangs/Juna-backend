using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Juna.Feed.DomainModel;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Util;
using Juna.Feed.DomainModel.Builder;
using Juna.Feed.Service.Helpers;


namespace Juna.Feed.Service
{
    public class JunaUserService
    {
        private JunaUserRepository _junaUserRepository;
        private Stream.StreamClient _streamClient;
        private ActivityRepository _activityRepository;

        public JunaUserService(JunaUserRepository junaUserRepository, Stream.StreamClient streamClient,
            ActivityRepository activityRepository)
        {
            _junaUserRepository = junaUserRepository;
            _streamClient = streamClient;
            _activityRepository = activityRepository;
        }

        public JunaUser GetJunaUserByObjectId(string objectId)
        {
            return _junaUserRepository.GetByObjectId(objectId);
        }

        public JunaUser Create(JunaUser junaUserInput)
        {
            var junaUser = junaUserInput;

            if (junaUser != null)
            {
                _junaUserRepository.Save(junaUserInput);
            }

            return junaUser;
        }

        public JunaUser CreateNewOrGetExistingUser(
            string email,
            string displayName,
            string country,
            string identityProvider,
            string city,
            string givenName,
            string postalCode,
            string jobTitle,
            string streetAddress,
            string surname,
            string userObjectID)
        {
            JunaUser junaUserCreated = _junaUserRepository.GetByObjectId(userObjectID);

            if (junaUserCreated == null)
            {
                junaUserCreated = _junaUserRepository.Save(new JunaUser
                {
                    Id = Guid.NewGuid(),
                    EmailAddress = email,
                    ObjectId = userObjectID,
                    DisplayName = displayName,
                    Country = country,
                    IdentityProvider = identityProvider,
                    City = city,
                    GivenName = givenName,
                    PostalCode = postalCode,
                    JobTitle = jobTitle,
                    StreetAddress = streetAddress,
                    Surname = surname
                });
            }

            return junaUserCreated;
        }

        public JunaUser GetJunaUserByEmail(string email, string objectId)
        {
            return _junaUserRepository.GetByEmailAndObjectId(email, objectId);
        }

        public void FollowUser(JunaUser user, JunaUser followedUser, DateTime time)
        {
            var activity = new ActivityBuilder()
                            .WithActor(user)
                            .WithVerb(InteractionMetadata.INTERACTION_FOLLOW)
                            .WithObject(followedUser)
                            .WithTime(time)
                            .Build();
            _activityRepository.Save(activity);
            var userFeed = _streamClient.Feed(FeedGroup.UserFeedType, user.ObjectId);
            var followUserFeed = _streamClient.Feed(FeedGroup.UserFeedType, followedUser.ObjectId);

            userFeed.FollowFeed(FeedGroup.UserFeedType, followedUser.ObjectId);
        }

        public void UnFollowUser(JunaUser user, JunaUser followedUser)
        {
            var activity = _activityRepository.GetByActorVerbAndObject(
                       actor: ActivityHelper.GetActor(user),
                       verb: InteractionMetadata.INTERACTION_FOLLOW,
                       objectString: ActivityHelper.GetObject(followedUser)
                    );
            var userFeed = _streamClient.Feed(FeedGroup.UserFeedType, user.ObjectId);
            userFeed.UnfollowFeed(FeedGroup.UserFeedType, followedUser.ObjectId);
            if (activity != null)
                _activityRepository.Delete(activity);
        }
    }
}
