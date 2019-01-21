using Microsoft.Extensions.Options;

namespace Juna.Feed.WebApp.Proxy
{
    public class TestServiceOptions : IOptions<TestServiceOptions>
    {
        public string BaseUrl { get; set; }
        public TestServiceOptions Value => this;
    }
}