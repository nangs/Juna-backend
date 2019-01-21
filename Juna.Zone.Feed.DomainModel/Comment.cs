using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.DomainModel
{
    public class Comment: Activity
    {
		[JsonProperty("message")]
		public string Message { get; set; }
        [JsonProperty("replyTo")]
        public string ParentCommentId { get; set; }
    }
}
