using System;
using Juna.Zone.RssFeed.RssFeedHelper;
using NLog;

namespace Juna.Zone.RssFeed
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            bool more = true;
            logger.Info("Welcome to Juna Zone RssFeed Loader...\n");
            while (more)
            {
                Console.Write("Please enter RSS url: ");
                // Read rss url
                string url = Console.ReadLine();
                try
                {
                    // Invoke rss reader with the given url
                    var rfReader = new RssFeedReader(url, new RssFeedLoader());
                    rfReader.ReadAndLoadRss();
                }
                catch (ArgumentNullException ex)
                {
                    logger.Info("Please provide a valid url..." + ex.Message);
                }
                Console.Write("Want to load more rss feeds?(y/n): ");
                var opt = Console.ReadLine();
                more =  opt.Equals("y") ? true : false;
            }
        }
    }
}
