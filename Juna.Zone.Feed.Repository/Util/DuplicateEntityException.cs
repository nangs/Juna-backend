using Juna.DDDCore.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Juna.Feed.Repository.Util
{
	public class DuplicateEntityException : Exception
	{
		public DuplicateEntityException(AggregateRoot entity) : this()
		{
			Trace.TraceInformation($"Entity with {entity.Id} already exists");
		}
		public DuplicateEntityException() : base()
		{
		}

		public DuplicateEntityException(string message) : base(message)
		{
		}

		public DuplicateEntityException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected DuplicateEntityException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
