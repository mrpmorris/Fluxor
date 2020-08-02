# DisposableCallback was not disposed

Components that descend from `FluxorComponent` or `FluxorLayout` automatically subscribe to the
`StateChanged` event on every `IState<T>` property in the component automatically. When the component
is disposed, this subscription is removed, to avoid memory leaks.

If ever you see an error message like the following

> DisposableCallback with Id "(Some text here)" was not disposed.

it is most likely because of one of the following reasons:

* You have overridden `Dispose(bool disposed)` on `FluxorComponent`
or `FluxorLayout` and not called `base.Dispose(disposed)`.

**Fix:** If you override a method, make sure you call the base method.

```C#
protected override void Dispose(bool disposed)
{
  base.Dispose(disposed);
  // etc
}
```

* In more advanced scenarios, you have called `IStore.BeginInternalMiddlewareChange` without
disposing the result.

**Fix:** If you execute a method that returns an IDisposable,
make sure you dispose it. Note that the following example is for
advanced use cases only (middleware).

```C#
using (Store.BeginMiddlewareChange())
{
	//etc
}
```