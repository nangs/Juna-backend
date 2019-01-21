using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Juna.Feed.Service.Test.Core
{
    public interface IFakeDocumentQuery<T> : IDocumentQuery<T>, IOrderedQueryable<T>
    {

    }

}
