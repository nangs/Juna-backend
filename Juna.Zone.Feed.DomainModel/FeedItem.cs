using Juna.DDDCore.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juna.Feed.DomainModel
{
	public class FeedItem : AggregateRoot
	{
		public const string NewsFeedItem = "News";
		public const string ImageFeedItem = "Image";
		public const string TweetFeedItem = "Tweet";
		public const string VideoFeedItem = "Video";
        public const string AudioFeedItem = "Audio";
        public const string RootCommentFeedItem = "Comment";
        private readonly string[] AllowedContentTypes = { NewsFeedItem, ImageFeedItem, TweetFeedItem, VideoFeedItem , AudioFeedItem , RootCommentFeedItem };

		[JsonProperty("id")]
		public override Guid Id { get; set; }
		[JsonProperty("title")]
		public string Title { get; set; }
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("source")]
		public string Source { get; set; }
        [JsonProperty("actor")]
        public JunaUser Actor { get; set; }
        [JsonProperty("datePublished")]
        public DateTime DatePublished { get; set; }
        [JsonProperty("summary")]
		public string Summary { get; set; }
		[JsonProperty("thumbnail")]
		public Image Thumbnail { get; set; }
		[JsonProperty("tags")]
		public List<string> Tags { get; set; }
		[JsonProperty("dateCreated")]
		public DateTime DateCreated { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("contentType")]
		public string ContentType { get; set; }
        [JsonProperty("comments")]
        public List<Comment> Comments { get; set; }
        [JsonProperty("interactions")]
		public InteractionMetadata Interactions { get; set; }
		[JsonIgnore]
		public override IReadOnlyList<IDomainEvent> DomainEvents { get { return base.DomainEvents; } }

		public FeedItem(string type)
		{

			ContentType = type;
		}

        
    }
	public class NewsFeedItem : FeedItem
	{
		public NewsFeedItem() : base(FeedItem.NewsFeedItem) { }
	}

	public class ImageFeedItem : FeedItem
	{
		public ImageFeedItem() : base(FeedItem.ImageFeedItem) { }
	}

	public class VideoFeedItem : FeedItem
	{
		public VideoFeedItem() : base(FeedItem.VideoFeedItem) { }
	}

    public class AudioFeedItem : FeedItem
    {
        public AudioFeedItem() : base(FeedItem.AudioFeedItem) { }
    }


    public class TweetFeedItem : FeedItem
	{
		public TweetFeedItem() : base(FeedItem.TweetFeedItem) { }
	}

    public class RootCommentFeedItem : FeedItem
    {
        public RootCommentFeedItem() : base(FeedItem.RootCommentFeedItem) {}
    }
}
