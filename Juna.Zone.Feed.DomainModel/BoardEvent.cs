using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.DomainModel
{
    public class BoardEvent
    {
		public const string FOOTBALL_MATCH = "FootballMatch";
		public const string TUNE_CONTEST = "TuneContest";
		public static readonly string[] EventTypes = { FOOTBALL_MATCH, TUNE_CONTEST };

		// todo: Cosmosdb vomits when querying because of this. Whatever serializer it is using 
		// is unable to deal with the fact that there's no empty constructor
//		public BoardEvent(string boardType, long foreignId)
//		{
//			if (!EventTypes.Any(e => e.Equals(boardType)))
//				throw new InvalidOperationException();
//		}

		// todo: valueobjects should not have public sets
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("foreignId")]
        public long ForeignId{ get; set; }
    }
}
