using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.DDDCore.Common
{
	public interface IHandler<T> where T : IDomainEvent
	{
		void Handle(T domainEvent);
	}
}
