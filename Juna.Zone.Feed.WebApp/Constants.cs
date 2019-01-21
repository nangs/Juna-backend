using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juna.Feed.WebApp
{
    public class Constants
    {
        public const string OpenIdConnectAuthenticationScheme = "OpenID Connect B2C";
        public const string B2CPolicy = "b2cPolicy";

        public const string AcrClaimType = "http://schemas.microsoft.com/claims/authnclassreference";
        public const string ObjectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public const string IdentityKey = "identity";
    }
}
