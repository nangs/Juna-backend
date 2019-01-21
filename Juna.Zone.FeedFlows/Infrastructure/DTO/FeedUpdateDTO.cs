using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Juna.Feed.DomainModel;

namespace Juna.FeedFlows.Infrastructure.DTO
{
	public class FeedUpdateDTO
	{
		//[{"new":[{"actor":"epl","foreign_id":"27f6193b-60dc-44eb-a819-31659cc714bc","id":"47b29200-1b32-11e8-8080-8000550aff74","object":"27f6193b-60dc-44eb-a819-31659cc714bc","origin":null,"target":"","time":"2018-02-26T20:19:00.000000","verb":"post"}],"deleted":[],"feed":"club_tournaments:epl","app_id":30564,"published_at":"2018-02-26T20:30:03.008Z"}]
		private const string Field_ActivitiesAdded = "new";
		private const string Field_ActivitiesDeleted = "deleted";
		private const string Field_DatePublished = "published_at";
		private const string Field_AppId = "app_id";
		private const string Field_Feed = "feed";

		readonly IDictionary<string, JToken> _data = new Dictionary<string, JToken>();
		public ActivityFeed Feed { get; set; }
		[JsonProperty(Field_DatePublished)]
		public DateTime? DatePublished { get; set; }
		[JsonProperty(Field_AppId)]
		public string AppId { get; set; }
		public IList<ActivityDTO> ActivitiesAdded { get; set; }
		public IList<ActivityDTO> ActivitiesDeleted { get; set; }
		[JsonConstructor]
		public FeedUpdateDTO()
		{
			ActivitiesAdded = new List<ActivityDTO>();
			ActivitiesDeleted = new List<ActivityDTO>();
		}

		private static bool IsBuiltInType(Type type)
		{
			return (type == typeof(object) || Type.GetTypeCode(type) != TypeCode.Object);
		}

		internal static FeedUpdateDTO FromJson(string json)
		{
			return FromJson(JObject.Parse(json));
		}

		public static FeedUpdateDTO FromJson(JObject obj)
		{
			var feedUpdate = new FeedUpdateDTO();

			var props = obj.Properties().Select(p => p.Name).ToArray();
			obj.Properties().ToList().ForEach((prop) =>
			{
				switch (prop.Name)
				{
					case Field_Feed: feedUpdate.Feed = new ActivityFeed(prop.Value.Value<string>());
						break;
					case Field_AppId: feedUpdate.AppId = prop.Value.Value<string>();
						break;
					case Field_DatePublished: feedUpdate.DatePublished = prop.Value.Value<DateTime>();
						break;
					case Field_ActivitiesAdded:
							ParseActivities(prop, feedUpdate.ActivitiesAdded);
							break;

					case Field_ActivitiesDeleted:
							ParseActivities(prop, feedUpdate.ActivitiesDeleted);
							break;
					default:
							// stash everything else as custom
							feedUpdate._data[prop.Name] = prop.Value;
							break;
				}
			});

			return feedUpdate;
		}

		private static void ParseActivities(JProperty prop, IList<ActivityDTO> activities)
		{
			JArray array = prop.Value as JArray;
			if ((array != null) && (array.Count > 0))
			{
				array.ToList().ForEach(a => activities.Add(ActivityDTO.FromJson((JObject)a)));
			}
		}
	}
}
