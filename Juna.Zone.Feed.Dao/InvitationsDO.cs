using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.Dao
{
   public  class InvitationsDO : CosmosDBEntity
    {
        [JsonProperty("boardId")]
        public string BoardId { get; set; }
        [JsonProperty("userId")]
        public string UserId { get; set; }
    }
}
