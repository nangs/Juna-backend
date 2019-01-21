using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.DDDCore.Common
{
	public abstract class AggregateRoot : DomainEntity
	{
		private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
		public virtual IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

		protected virtual void AddDomainEvent(IDomainEvent newEvent)
		{
			_domainEvents.Add(newEvent);
		}

		public virtual void ClearEvents()
		{
			_domainEvents.Clear();
		}
	}
}
