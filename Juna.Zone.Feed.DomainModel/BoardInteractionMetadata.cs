using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.DomainModel
{
    public class BoardInteractionMetadata: InteractionMetadata
    {
		public const string BOARD_INTERACTION_ENTER = "enter";
		public const string BOARD_INTERACTION_LEAVE = "leave";
        public const string ACTIVE_BOARD_USERS = "activeUsers";
        public const string BOARD_INTERACTION_INVITE = "invite";
        [JsonProperty("followers")]
		public long Followers { get; set; }
        [JsonProperty("activeUsers")]
        public long ActiveUsers { get; set; }
    }
}
