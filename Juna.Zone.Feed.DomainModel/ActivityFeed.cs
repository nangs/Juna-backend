using Juna.DDDCore.Common;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juna.Feed.DomainModel
{
	public class ActivityFeed : AggregateRoot
    {
		public ActivityFeed(string feedString)
		{
			// todo: Move this validation to the infrastructure layer because FeedGroup will need a db call
			if (string.IsNullOrEmpty(feedString)) throw new InvalidOperationException("Need a proper feed string");
			var feedNameComponents = feedString.Split(':');
			if (feedNameComponents.Length > 2) throw new InvalidOperationException("Too many segments");
			if (feedNameComponents.Length == 2)
			{
				// todo: Remove the group assignment from here and read it from the string itself.
				// Or move the extraction of the feedgroup from here to a service layer method
				//Group = new FeedGroup(name: feedNameComponents[0], type: FeedGroup.ClubTournamentsFeedGroup.ToString());
				Name = feedNameComponents[1];
			}
			else
			{
				Name = feedString;
			}
		}
		public string Name { get; set; }
		public FeedGroup Group { get; set; }
    }
}
