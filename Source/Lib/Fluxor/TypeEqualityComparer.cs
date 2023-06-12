using System.Collections.Generic;

namespace Fluxor
{
	internal class TypeEqualityComparer<T> : IEqualityComparer<T>
	{
		public bool Equals(T x, T y) => x?.GetType() == y?.GetType();

		public int GetHashCode(T obj) => obj.GetType().GetHashCode();
	}
}
