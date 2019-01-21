using Juna.Feed.Dao;
using System.Collections.Generic;

namespace Juna.Feed.Repository.Util
{
	// todo: Maybe move this to a common folder
	public class DocumentDbQueryResult<T> where T : CosmosDBEntity
	{
		public DocumentDbQueryResult(string token, List<T> values)
		{
			DbToken = token;
			Values = values;
		}

		public IList<T> Values { get; private set; }
		public string DbToken { get; private set; }
    }
}
