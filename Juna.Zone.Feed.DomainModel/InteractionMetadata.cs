using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.DomainModel
{
    public class InteractionMetadata
    {
		public const string INTERACTION_LIKE = "like";
		public const string INTERACTION_SHARE = "share";
		public const string INTERACTION_PIN = "pin";
        public const string INTERACTION_COMMENT = "comment";
        public const string INTERACTION_POST = "post";
        public const string INTERACTION_BLOCK = "block";
        public const string INTERACTION_BAN = "ban";
        public const string INTERACTION_MUTE = "mute";
        public const string INTERACTION_REPORT = "report";
        public const string INTERACTION_DISLIKE = "dislike";
        public const string INTERACTION_FOLLOW = "follow";
        public const string INTERACTION_UNFOLLOW = "unfollow";

        [JsonProperty("likes")]
		public int Likes { get; set; }
        [JsonProperty("dislikes")]
        public int Dislikes { get; set; }
        [JsonProperty("shares")]
		public int Shares { get; set; }
		[JsonProperty("pins")]
		public int Pins { get; set; }
        [JsonProperty("comments")]
        public int Comments { get; set; }
        [JsonProperty("posts")]
        public int Posts { get; set; }
        [JsonProperty("blocks")]
        public int Blocks { get; set; }
        [JsonProperty("bans")]
        public int Bans { get; set; }
        [JsonProperty("mutes")]
        public int Mutes { get; set; }
        [JsonProperty("reports")]
        public int Reports { get; set; }
    }
}
