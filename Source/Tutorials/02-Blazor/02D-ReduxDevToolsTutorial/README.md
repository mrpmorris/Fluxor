# Fluxor - Blazor Web Tutorials

## Redux Dev Tools

[Redux Dev Tools][ReduxDevToolsLink] is a browser plugin for
[Chrome][ChromePluginLink] and [Firefox][FirefoxPluginLink].

![](./../../../../images/redux-dev-tools.jpg)

**NOTE:** ReduxDevTools allows the user to alter the state of your store
directly. This might be a security flaw, so you should only reference
this package in `Debug` builds.

To enable Fluxor integration, follow these steps
 1. Add the [Fluxor.Blazor.Web.ReduxDevTools][FluxorReduxDevToolsLink] nuget package
    to your project. Make sure you make it conditional on `DEBUG` mode.

```
 <ItemGroup Condition="$(Configuration)=='Debug'">
    <PackageReference Include="Fluxor.Blazor.Web.ReduxDevTools" Version="...." />
  </ItemGroup>
```

 2. Use the `UseReduxDevTools` extension on the Fluxor options.

```c#
services.AddFluxor(o =>
    {
        o.ScanAssemblies(typeof(SomeType).Assembly);
#if DEBUG
        o.UseReduxDevTools();
#endif
    });
```

 3. When you run your application, click the icon for the Redux Dev Tools extension.
    Or, open the the brower's developer tools window and find the Redux tab.


### Additional options

When calling `UseReduxTools` it is possible to pass additional options to the
Redux Dev Tools browser plugin.

```c#
services.AddFluxor(o =>
  o.ScanAssemblies(typeof(SomeType).Assembly),
  o.UseReduxDevTools(rdt =>
    {
      rdt.Name = "My application";
    }));
```


 * Name: The name to display.
 * Latency: How often actions are added to the plugin.
 * MaximumHistoryLength: How many actions at most should be displayed in the plugin.

There is also an option to have Fluxor pass the strack trace to `ReduxDevTools`. This is useful
for when an action is being dispatched and you need to identify where it was dispatched from.

```c#
o.UseReduxDevTools(rdt =>
  {
    rdt.Name = "My application";
    rdt.EnableStackTrace();
  }));
```

Options for `EnableStackTrace` are
 * limit: The maximum number of stack frames to include in the trace
 * stackTraceFilterExpression:
     * Allows you to specify a regular expression that will be used to filter the stack trace.
     * Only lines that match the expression will be included in the trace.
     * The default expression will exlude System, Microsoft, and Fluxor ReduxDevTools Middleware.
     * To include all stack frames, set the expression to "".

Note that determining the stack trace is an expensive operation.

![](./../../../../images/redux-dev-tools-trace.jpg)

 [ReduxDevToolsLink]: https://github.com/zalmoxisus/redux-devtools-extension
 [ChromePluginLink]: https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd?hl=en
 [FirefoxPluginLink]: https://addons.mozilla.org/en-GB/firefox/addon/reduxdevtools/
 [FluxorReduxDevToolsLink]: https://www.nuget.org/packages/Fluxor.Blazor.Web.ReduxDevTools/
