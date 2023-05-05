using System;
using System.Collections.Generic;
using System.Text;

namespace Fluxor.StoreBuilderSourceGenerator;

public readonly struct Either<L, R>
{
	public readonly bool IsLeft;
	public bool IsRight => !IsLeft;

	private readonly L _Left;
	public L Left => IsLeft ? _Left : throw new InvalidOperationException("Either is not in the Left state.");

	private readonly R _Right;
	public R Right => !IsLeft ? _Right : throw new InvalidOperationException("Either is not in the Right state.");

	private Either(L left)
	{
		_Left = left;
		_Right = default(R);
		IsLeft = true;
	}

	private Either(R right)
	{
		_Left = default(L);
		_Right = right;
		IsLeft = false;
	}

	public static implicit operator Either<L, R>(L left) => new Either<L, R>(left);

	public static implicit operator Either<L, R>(R right) => new Either<L, R>(right);

	public T Match<T>(Func<L, T> leftFunc, Func<R, T> rightFunc)
	{
		return IsLeft ? leftFunc(_Left) : rightFunc(_Right);
	}
}

