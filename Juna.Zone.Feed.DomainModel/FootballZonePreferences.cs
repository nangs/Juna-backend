using System;
using System.Collections.Generic;
using System.Text;
using Juna.DDDCore.Common;
using Juna.Feed.DomainModel;
using Newtonsoft.Json;

namespace Juna.Feed.DomainModel
{
    public class FootballZonePreferences : IZonePreferences
    {
        public FootballZonePreferences()
        {
            Leagues = new List<string>();
            Teams = new List<string>();
        }

        [JsonProperty("Leagues")]
        public List<string> Leagues { get; set; }

        [JsonProperty("Teams")]
        public List<string> Teams { get; set; }
    }

    public interface IZonePreferences
    {
        List<string> Leagues { get; set; }

        List<string> Teams { get; set; }
    }
}
