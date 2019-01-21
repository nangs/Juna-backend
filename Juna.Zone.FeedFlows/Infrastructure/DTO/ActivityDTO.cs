using Juna.DDDCore.Common;
using Newtonsoft.Json.Linq;
using Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juna.FeedFlows.Infrastructure.DTO
{
	// The reason we have a DTO for activity is that we dont' have a way to insert the Id
	// field into the Activity class. This is something the streamclient does, and the 
	// stream-net library doesn't give us access to a setter method for Id, but we need the
	// Id field to be able to process the Update webhooks we get from every stream insertion
    public class ActivityDTO: ValueObject<ActivityDTO>
    {
		private const string Field_Id = "id";
		private const string Field_Actor = "actor";
		private const string Field_Verb = "verb";
		private const string Field_Object = "object";
		private const string Field_ForeignId = "foreign_id";
		private const string Field_To = "to";
		private const string Field_Target = "target";
		private const string Field_Time = "time";
		private const string Field_ActorCount = "actor_count";
		private const string Field_CreatedAt = "created_at";
		private const string Field_UpdatedAt = "updated_at";
		private const string Field_Group = "group";
		public static readonly string[] ActivityFields = {
			Field_Id, Field_Actor, Field_Verb, Field_Object, Field_ForeignId,
			Field_To, Field_Target, Field_Time, Field_ActorCount, Field_CreatedAt,
			Field_UpdatedAt, Field_Group
		}; 

		public string Id { get; private set; }
		public string Actor { get; private set; }
		public string ForeignId { get; private set; }
		public string Object { get; private set; }
		public string Target { get; private set; }
		public string To { get; private set; }
		public string Verb { get; private set; }
		public string Time { get; private set; }
		public ActivityDTO(
			string id, 
			string actor,
			string foreignId,
			string sObject,
			string target,
			string to,
			string verb,
			string time)
		{
			Id = id;
			Actor = actor;
			ForeignId = ForeignId;
			Object = sObject;
			Target = target;
			To = to;
			Verb = verb;
			Time = time;
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Id;
			yield return Actor;
			yield return ForeignId;
			yield return Object;
			yield return Target;
			yield return To;
			yield return Verb;
			yield return Time;
		}

		internal static ActivityDTO FromJson(JObject obj)
		{
			var props = obj.Properties().Select(p => p.Name).ToArray();
			var fieldMap = new Dictionary<string, string>();
			obj.Properties().ToList().ForEach((prop) => fieldMap.Add(prop.Name, prop.Value.Value<string>()));
			return new ActivityDTO(
				id: fieldMap.ContainsKey(Field_Id) ? fieldMap[Field_Id] : null,
				actor: fieldMap.ContainsKey(Field_Actor) ? fieldMap[Field_Actor] : null,
				foreignId: fieldMap.ContainsKey(Field_ForeignId) ? fieldMap[Field_ForeignId] : null,
				sObject: fieldMap.ContainsKey(Field_Object) ? fieldMap[Field_Object] : null,
				target: fieldMap.ContainsKey(Field_Target) ? fieldMap[Field_Target] : null,
				to: fieldMap.ContainsKey(Field_To) ? fieldMap[Field_To] : null,
				verb: fieldMap.ContainsKey(Field_Verb) ? fieldMap[Field_Verb] : null,
				time: fieldMap.ContainsKey(Field_Time) ? fieldMap[Field_Time] : null);
		}
	}
}
