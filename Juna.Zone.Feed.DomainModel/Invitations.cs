using Juna.DDDCore.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.DomainModel
{
   public class Invitations : AggregateRoot
    { 
        [JsonProperty("boardId")]
        public Guid BoardId { get; set; }
        [JsonProperty("userId")]
        public string UserId { get; set; }
    }
}
