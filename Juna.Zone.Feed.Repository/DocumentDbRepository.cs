using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Juna.DDDCore.Common;
using Juna.Feed.Dao;
using Juna.Feed.Repository.Util;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Juna.Feed.Repository
{
    public static class RepositoryConstants
    {
        public const string ContinuationTokenHeader = "newsfeed-continuation-token";

    }

    // todo: Write integration tests for this module
    public abstract class DocumentDbRepository<TDomainEntity, TPersistentEntity> : Repository<TDomainEntity> 
		where TDomainEntity : AggregateRoot 
		where TPersistentEntity : CosmosDBEntity
    {
		private readonly string CollectionName;
		protected DocumentDbUtil DbUtil;
        public DocumentDbRepository(
			DocumentDbUtil documentDbUtil,
			string collectionName) : this()
		{
			CollectionName = collectionName;
			DbUtil = documentDbUtil;
		}

		protected DocumentDbRepository(){}

		protected async Task<DocumentDbQueryResult<TPersistentEntity>> QueryAndContinue(
			
            string continuationToken,
			int take,
			string filter,
			string specificFields = null,
			string orderBy = null,
			string partitionKey = null)
		{
			if (specificFields == null)
				specificFields = "*";

			var queryOptions = new FeedOptions { MaxItemCount = take };
			if (string.IsNullOrEmpty(partitionKey))
				queryOptions.EnableCrossPartitionQuery = true;
			else
				queryOptions.PartitionKey = new PartitionKey(partitionKey);

			var link = UriFactory.CreateDocumentCollectionUri(DbUtil.DatabaseName, CollectionName);
			var type = typeof(TPersistentEntity);
			var query = $"SELECT {specificFields} FROM {CollectionName} a";
			query += $" WHERE a.type = \"{type.ToString()}\"";
			if (!string.IsNullOrEmpty(filter))
				query += $" AND ({filter})";
			if (!string.IsNullOrEmpty(orderBy))
				query += $" ORDER BY a.{orderBy}";

			Trace.TraceInformation($"Running query: {query}");

			if (!string.IsNullOrEmpty(continuationToken))
			{
				queryOptions.RequestContinuation = continuationToken;
			}
			var dquery = DbUtil.Client.CreateDocumentQuery<TPersistentEntity>(link, query, queryOptions)
					.AsDocumentQuery();

			string queryContinuationToken = null;
			var page = await dquery.ExecuteNextAsync<TPersistentEntity>();
			var queryResult = page.ToList();
			if (dquery.HasMoreResults)
				queryContinuationToken = page.ResponseContinuation;

			return new DocumentDbQueryResult<TPersistentEntity>(queryContinuationToken, queryResult);
		}

		public abstract TDomainEntity[] ConvertToDomainEntities(TPersistentEntity[] daos);
	}
}
