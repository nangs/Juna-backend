using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.FeedFlows.Util
{
    public class AppConfiguration
    {
        public AppSettings AppSettings { get; set; }
    }

    public class AppSettings
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string AuthorityFormat { get; set; }
        public string RedirectUri { get; set; }
        public string JunaCreateFeedItems { get; set; }
        public string JunaRestApiEndpoint { get; set; }
        public string NewsFeedApiKey { get; set; }
        public string BoardActivationNotifierKey { get; set; }
    }
}
