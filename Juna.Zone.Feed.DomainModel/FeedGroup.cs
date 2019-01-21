using Juna.DDDCore.Common;
using System.Collections.Generic;

namespace Juna.Feed.DomainModel
{
	public sealed class FeedGroup: DomainEntity
	{
		// These are feed types defined by stream.io
		public const string BoardFeedType = "Board";
		public const string UserFeedType = "user";
		public const string CardFeedType = "Card";
        public const string JunaFeedType = "junaFeed";
        public string Name { get; set; }
		public string Type { get; set; }

		public FeedGroup(string name, string type)
		{
			Name = name;
			Type = type;
		}
	}
}
