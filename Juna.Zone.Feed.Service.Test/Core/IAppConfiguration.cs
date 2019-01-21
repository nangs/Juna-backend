using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.Service.Test.Core
{
    public interface IAppConfiguration
    {
        Logging Logging { get; }
        AppSettings AppSettings { get; }
        AzureMediaServices AzureMediaServices { get; }
    }
}
