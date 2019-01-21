using Stream;
using System.Configuration;
using System;
using NLog;
using Juna.Zone.Data.Model;

namespace Juna.Zone.RssFeed.RssFeedHelper
{
    public class RssFeedLoader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private StreamClient streamCLient;
        public RssFeedLoader()
        {
            this.streamCLient = new StreamClient(ConfigurationManager.AppSettings["GetStreamKey"], ConfigurationManager.AppSettings["GetStreamSecret"]);
        }

        /// <summary>
        /// Loads rss feed to GetStream feed group `zone_rss_feed`
        /// </summary>
        /// <param name="rssFeed"></param>
        public void LoadFeed(ZoneRssFeed rssFeed)
        {
            var feed = this.streamCLient.Feed(ZoneRssFeed.FeedGroup, GetFeedId(rssFeed.Item.PublishDate));
            logger.Info("Loading Feed: {0}", rssFeed.Item.Title.Text);
            feed.AddActivity(rssFeed.SetData(rssFeed.Item));

        }

        /// <summary>
        /// Takes rss publish date and provides GetStream feed identity 
        /// </summary>
        /// <param name="dateOffset">Rss feed publish date</param>
        /// <returns>Rss Feed identity for GetStream</returns>
        private string GetFeedId(DateTimeOffset dateOffset)
        {
            return string.Format("{0}-{1}-{2}", dateOffset.Day.ToString().PadLeft(2, '0'), dateOffset.Month.ToString().PadLeft(2, '0'), dateOffset.Year);
        }
    }
}
