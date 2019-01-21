using AutoMapper;
using Juna.Feed.DomainModel;
using Juna.Feed.Dao;
using Juna.Feed.Repository.Util;
using Microsoft.Azure.Documents.Client;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Juna.Feed.Repository
{
	public class FeedItemRepository : DocumentDbRepository<FeedItem, FeedItemDO>
	{
		private IMapper _mapper;
		private Uri CollectionUri { get; set; }

		public FeedItemRepository(DocumentDbUtil documentDbUtil, string collectionName, IMapper mapper)
			: base(documentDbUtil, collectionName)
		{
			_mapper = mapper;
			CollectionUri = UriFactory.CreateDocumentCollectionUri(documentDbUtil.DatabaseName, collectionName);
		}

		public override FeedItem GetById(Guid Id)
		{
			var item = DbUtil.Client.CreateDocumentQuery<FeedItemDO>(CollectionUri)
							.Where(n => n.Id.Equals(Id.ToString()) && n.Type == typeof(FeedItemDO).ToString())
							.AsEnumerable().FirstOrDefault();
			return item == null
				? null
				: _mapper.Map<FeedItem>(item);
		}

        public List<FeedItem> GetByObjectId(List<string> objectIds)
        {
            var item = DbUtil.Client.CreateDocumentQuery<FeedItemDO>(CollectionUri)
            .Where(n => objectIds.Contains(n.Id) && n.Type == typeof(FeedItemDO).ToString())
            .AsEnumerable().OrderByDescending(n => n.DateCreated).ToList();
            return item == null
            ? null
             : _mapper.Map<List<FeedItem>>(item);
        }


        public FeedItem GetByUrl(string url)
		{
			var item = DbUtil.Client.CreateDocumentQuery<FeedItemDO>(CollectionUri)
							.Where(n => url.Equals(n.Url) && n.Type == typeof(FeedItemDO).ToString())
							.AsEnumerable().FirstOrDefault();
			return item == null
				? null
				: _mapper.Map<FeedItem>(item);
		}

		// todo: Figure out a way to make this async
		public override FeedItem Save(FeedItem feedItem)
		{
			var feedItemDO = _mapper.Map<FeedItemDO>(feedItem);
			feedItemDO.DateCreated = DateTime.UtcNow;
			// todo: This contains a major flaw. What if the type method doesn't match the class name?
			// There are not checks currently
			// todo: Move to a factory method or a builder method
			feedItemDO.Type = feedItemDO.GetType().ToString();
			feedItemDO.Id = Guid.NewGuid().ToString();
			feedItemDO.DateCreated = DateTime.UtcNow;
			Trace.TraceInformation($"Inserting new feed item with id[{feedItemDO.Id}]");
			DbUtil.Client.CreateDocumentAsync(CollectionUri, feedItemDO);
			feedItem = _mapper.Map<FeedItem>(feedItemDO);
			return feedItem;
		}

        public override async Task<FeedItem> SaveAsync(FeedItem feedItem)
        {
            var feedItemDO = _mapper.Map<FeedItemDO>(feedItem);
            feedItemDO.DateCreated = DateTime.UtcNow;
            // todo: This contains a major flaw. What if the type method doesn't match the class name?
            // There are not checks currently
            // todo: Move to a factory method or a builder method
            feedItemDO.Type = feedItemDO.GetType().ToString();
            feedItemDO.Id = Guid.NewGuid().ToString();
            feedItemDO.DateCreated = DateTime.UtcNow;
            Trace.TraceInformation($"Inserting new feed item with id[{feedItemDO.Id}]");
            await DbUtil.Client.CreateDocumentAsync(CollectionUri, feedItemDO);
            feedItem = _mapper.Map<FeedItem>(feedItemDO);

            return feedItem;
        }

        public FeedItem Upsert(FeedItem feedItem)
		{
			var feedItemDO = _mapper.Map<FeedItemDO>(feedItem);
			feedItemDO.Type = feedItemDO.GetType().ToString();
			Trace.TraceInformation($"Upserting feed item with id [{ feedItem.Id }]");
			DbUtil.Client.UpsertDocumentAsync(CollectionUri, feedItemDO);
			feedItem = _mapper.Map<FeedItem>(feedItemDO);
			return feedItem;
		}

		// Praneeth - This is the only PersistentRepository method that exposes the Data Object
		// todo: Find a way to manipulate the inheritance hierarchy to return Feeditem
		// which is a domain object, but QueryAndContinueAsync has nothing to do with domain
		public async Task<DocumentDbQueryResult<FeedItemDO>> QueryAndContinueAsync(string continuationToken)
		{
			// todo: This could become a performance issue later on
			// todo: Ugly hack for orderBy descending Ugh!!
			return await base.QueryAndContinue(
					continuationToken, 20, null, null, "datePublished DESC", null);	
		}

		public override FeedItem[] ConvertToDomainEntities(FeedItemDO[] daos)
		{
			return daos?.Select(d => _mapper.Map<FeedItem>(d)).ToArray();
		}
	}
}