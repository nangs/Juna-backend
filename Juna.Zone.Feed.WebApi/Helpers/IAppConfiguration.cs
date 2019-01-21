using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juna.Feed.WebApi.Helpers
{
    public interface IAppConfiguration
    {
        Logging Logging { get; }
        AppSettings AppSettings { get; }
        AzureMediaServices AzureMediaServices { get; }
    }
}
