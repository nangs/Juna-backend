using System;

namespace Juna.Feed.DomainModel.Builder
{
    public class ActivityBuilder
    {
        private Activity _instance;
        public ActivityBuilder()
        {
            _instance = new Activity { Id = Guid.NewGuid()};
        }

        public ActivityBuilder WithActor(JunaUser user)
        {
            _instance.Actor = $"JunaUser:{user.ObjectId}";
            return this;
        }

        public ActivityBuilder WithTarget(Board board)
        {
            _instance.Target = $"Board-{board.Id}";
            return this;
        }

        public ActivityBuilder WithTarget(JunaUser user)
        {
            _instance.Target = $"JunaUser-{user.ObjectId}";
            return this;
        }

        public ActivityBuilder WithVerb(string verb)
        {
            _instance.Verb = verb;
            return this;
        }

        public ActivityBuilder WithObject(Board board)
        {
            _instance.Object = $"Board-{board.Id}";
            return this;
        }
        public ActivityBuilder WithObject(JunaUser user)
        {
            _instance.Object = $"JunaUser:{user.ObjectId}";
            return this;
        }
        public ActivityBuilder WithObject(FeedItem feedItem)
        {
            _instance.Object = $"{feedItem.ContentType}:{feedItem.Id}";
            return this;
        }
        public ActivityBuilder WithObject(Comment comment)
        {
            _instance.Object = $"Comment:{comment.Id}";
            return this;
        }
        public ActivityBuilder WithForeignId(Board board)
        {
            _instance.Object = $"Board:{board.Id}";
            return this;
        }
        public ActivityBuilder WithForeignId(JunaUser user)
        {
            _instance.Object = $"JunaUser:{user.Id}";
            return this;
        }
        public ActivityBuilder WithForeignId(Comment comment)
        {
            _instance.Object = $"Comment:{comment.Id}";
            return this;
        }
        public ActivityBuilder WithForeignId(FeedItem feedItem)
        {
            _instance.Object = $"feedItem:{feedItem.Id}";
            return this;
        }
        public ActivityBuilder WithTime(DateTime time)
        {
            _instance.Time = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            return this;
        }

        public Activity Build()
        {
            return _instance;
        }
    }
}
