using Juna.Feed.DomainModel;

namespace Juna.Feed.Service.Helpers
{
	public static class ActivityHelper
    {
		public static string GetActor(string botActor) => $"JunaUser:{botActor}";
		public static string GetActor(JunaUser user) => GetJunaUserStreamString(user);
		public static string GetObject(JunaUser user) => GetJunaUserStreamString(user);
		public static string GetObject(Board board) => GetBoardStreamString(board);
		public static string GetObject(FeedItem feedItem) => $"{feedItem.ContentType}:{feedItem.Id}";
		public static string GetTarget(Board board) => GetBoardStreamString(board);
		public static string GetTarget(JunaUser user) => GetJunaUserStreamString(user);
		private static string GetJunaUserStreamString(JunaUser user) => $"JunaUser:{user.ObjectId}";
		private static string GetBoardStreamString(Board board) => $"Board-{board.Id}";
	}
}
