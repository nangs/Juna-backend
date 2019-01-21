using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.DomainModel.Builder
{
   public class CommentBuilder 
    {
        private Comment _instance;
        public CommentBuilder()
        {
            _instance = new Comment { Id = Guid.NewGuid() };
        }

        public CommentBuilder WithActor(JunaUser user)
        {
            _instance.Actor = $"JunaUser:{user.ObjectId}";
            return this;
        }

        public CommentBuilder WithTarget(Board board)
        {
            _instance.Target = $"Board-{board.Id}";
            return this;
        }

        public CommentBuilder WithVerb(string verb)
        {
            _instance.Verb = verb;
            return this;
        }
        public CommentBuilder WithMessage(string message)
        {
            _instance.Message = message;
            return this;
        }
        public CommentBuilder WithParentCommentId(Comment comment)
        {
            _instance.ParentCommentId = $"Comment:{comment.Id}";
            return this;
        }

        public CommentBuilder WithObject(Board board)
        {
            _instance.Object = $"Board-{board.Id}";
            return this;
        }
        public CommentBuilder WithObject(JunaUser user)
        {
            _instance.Object = $"JunaUser:{user.Id}";
            return this;
        }
        public CommentBuilder WithObject(FeedItem feedItem)
        {
            _instance.Object = $"{feedItem.ContentType}:{feedItem.Id}";
            return this;
        }
        public CommentBuilder WithObject(Comment comment)
        {
            _instance.Object = $"Comment:{comment.Id}";
            return this;
        }
        public CommentBuilder WithForeignId(Board board)
        {
            _instance.Object = $"Board:{board.Id}";
            return this;
        }
        public CommentBuilder WithForeignId(JunaUser user)
        {
            _instance.Object = $"JunaUser:{user.Id}";
            return this;
        }
        public CommentBuilder WithForeignId(Comment comment)
        {
            _instance.Object = $"Comment:{comment.Id}";
            return this;
        }
        public CommentBuilder WithForeignId(FeedItem feedItem)
        {
            _instance.Object = $"feedItem:{feedItem.Id}";
            return this;
        }
        public CommentBuilder WithTime(DateTime time)
        {
            _instance.Time = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            return this;
        }


        public Comment Build()
        {
            return _instance;
        }
    }
}
