using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Juna.DDDCore.Common
{
	public abstract class Repository<T> where T : AggregateRoot
	{
		public abstract T GetById(Guid Id);

		public abstract T Save(T aggregateRoot);

        public abstract Task<T> SaveAsync (T aggregateRoot);
    }
}
