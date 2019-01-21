
using System;
using System.ServiceModel.Syndication;
using System.Xml;
using Juna.Zone.Data.Model;
using NLog;

namespace Juna.Zone.RssFeed.RssFeedHelper
{
    public class RssFeedReader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private XmlReader xmlReader;
        private RssFeedLoader rssFeedLoader;
        public RssFeedReader(string url, RssFeedLoader rfLoader)
        {
            xmlReader = XmlReader.Create(url);
            rssFeedLoader = rfLoader;
        }

        /// <summary>
        /// Reads rss feeds from the given url
        /// </summary>
        public void ReadAndLoadRss()
        {
            logger.Info("Started reading feeds...");

            // Load feeds
            SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
            logger.Info("Read rss feed completed...");
            xmlReader.Close();
            // Print feeds
            foreach(SyndicationItem item in feed.Items)
            {
                rssFeedLoader.LoadFeed(new ZoneRssFeed(item));
            }
        }

    }
}
