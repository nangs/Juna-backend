using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.Dao
{
    public class CommentDO: ActivityDO
    {
		[JsonProperty("message")]
		public string Message { get; set; }
        // todo: Convert ParentCommentId to Guid
        // Automapper started crapping out when ParentCommentId was Guid. It used to work before but stopped working. 
        // Need to investigate and revert to Guid later.
		[JsonProperty("replyTo")]
		public string ParentCommentId { get; set; }
    }
}
