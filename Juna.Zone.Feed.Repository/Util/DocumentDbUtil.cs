using Microsoft.Azure.Documents.Client;
using System;

namespace Juna.Feed.Repository.Util
{
	public class DocumentDbUtil
	{
		public string EndpointUrl { get; private set; }
		public string DbPrimaryKey { get; private set; }
		public string DatabaseName { get; private set; }
		public DocumentClient Client { get; set; }

		public DocumentDbUtil(string endpointUrl, string primaryKey, string databaseName)
		{
			EndpointUrl = endpointUrl;
			DbPrimaryKey = primaryKey;
			DatabaseName = databaseName;
			Client = new DocumentClient(new Uri(EndpointUrl), DbPrimaryKey);
		}
	}
}