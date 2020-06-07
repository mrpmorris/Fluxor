# Upgrading instructions

## Blazor.Fluxor (v1 or v2) to Fluxor (v3)
1. Uninstall the NuGet package Blazor.Fluxor and install the following:
  * Fluxor
  * Fluxor.Blazor.Web
  * Fluxor.Blazor.Web.ReduxDevTools (if used in your app)
2. Change the following namespaces in your source code
  * `Blazor.Fluxor` to `Fluxor`
  * `Blazor.Fluxor.Components` to `Fluxor.Blazor.Web.Components`
  * `Blazor.Fluxor.Routing` to `Fluxor.Blazor.Web.Middlewares.Routing`
  * `Blazor.Fluxor.ReduxDevTools` to `Fluxor.Blazor.Web.ReduxDevTools`
3. When bootstrapping Fluxor, change the options in `services.AddFluxor` as follows
  * Change `UseDependencyInjection` to `ScanAssemblies`
  * Change `AddMiddleware<Blazor.Fluxor.Routing.RoutingMiddleware>();` to `UseRouting();`
  * Change `AddMiddleware<Blazor.Fluxor.ReduxDevTools.ReduxDevToolsMiddleware>();` to `UseReduxDevTools();`
4. Instead of using `<Blazor.Fluxor.StoreInitializer/>` or calling `Store.Initialize()` in your razor file, add the following to your `App.razor` file
  * `<Fluxor.Blazor.Web.StoreInitializer/>`
5: `FluxorComponent` now implements `IDisposable`, so override `Dispose(bool disposing)` instead of implementing `IDisposable` yourself.
6: Change the script reference from `_content/Blazor.Fluxor/index.js` to `_content/Fluxor.Blazor.Web/scripts/index.js`
7: The `Go` class in the Routing namespace is now called `GoAction`

