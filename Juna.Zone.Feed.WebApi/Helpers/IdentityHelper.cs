using System.Security.Claims;

namespace Juna.Feed.WebApi.Helpers
{
    public class IdentityHelper
    {
		private IAppConfiguration _appConfig;
		public IdentityHelper(IAppConfiguration appConfig)
		{
			_appConfig = appConfig;
		}
		public string GetObjectId (ClaimsIdentity identity)
		{
			return identity.FindFirst(_appConfig.AppSettings.OidClaim).Value;
		}

		public bool IsAuthenticated(ClaimsIdentity identity)
		{
			return identity.IsAuthenticated;
		}
    }
}
