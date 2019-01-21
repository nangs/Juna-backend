using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.DDDCore.Common
{
	public abstract class DomainEntity
	{
		// todo: The set is supposed to be protected. 
		// What domain entity rule are we violating?
		public virtual Guid Id { get; set; }

		public override bool Equals(object obj)
		{
			var other = obj as DomainEntity;

			if (other is null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (GetType() != other.GetType())
				return false;

			return Id == other.Id;
		}

		public static bool operator ==(DomainEntity a, DomainEntity b)
		{
			if (a is null && b is null)
				return true;

			if (a is null || b is null)
				return false;

			return a.Equals(b);
		}

		public static bool operator !=(DomainEntity a, DomainEntity b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return (GetType().ToString() + Id).GetHashCode();
		}
	}
}
