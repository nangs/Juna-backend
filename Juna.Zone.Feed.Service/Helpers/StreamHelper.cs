using Juna.Feed.DomainModel;
using System;
using System.Linq;

namespace Juna.Feed.Service.Helpers
{
    public static class StreamHelper
    {
        public static string GetStreamActorId(Activity activity)
        {
            return activity.Actor.Split(':').Skip(1).FirstOrDefault();
        }

		public static string GetStreamObjectId(Activity activity)
		{
			return activity.Object.Split(':').Skip(1).FirstOrDefault();
		}

		public static string GetStreamActorId(Stream.Activity activity)
		{
			return activity.Actor.Split(':').Skip(1).FirstOrDefault();
		}

		public static string GetStreamObjectId(Stream.Activity activity)
		{
			return activity.Object.Split(':').Skip(1).FirstOrDefault();
		}

		public static string GetStreamTargetId(Activity activity)
		{
			return activity.Target.Split(':').Skip(1).FirstOrDefault();
		}

		// todo: move this to ActivityHelper
		public static string GetCardTarget(JunaUser user)
		{
			return $"Card:{user.ObjectId}";
		}
    }
}
