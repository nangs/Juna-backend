using System;
using System.Collections.Generic;
using System.Text;
using Juna.DDDCore.Common;
using Newtonsoft.Json;

namespace Juna.Feed.DomainModel
{
    public class UserPreference 
    {
        [JsonProperty("zone")]
        public Zone Zone { get; set; }

        [JsonProperty("FootballZone")]
        public IZonePreferences FootballZone { get; set; }
    }
}
