using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juna.Feed.WebApi.Helpers
{
    public class AuthenticationOptions
    {
        public string Instance { get; set; }
        public string TenantId { get; set; }

        public string Authority => $"{Instance}{TenantId}/v2.0";

        public string Audience { get; set; }
        public string SignInOrSignUpPolicy { get; set; }

        public string ApiName { get; set; }
    }
}
