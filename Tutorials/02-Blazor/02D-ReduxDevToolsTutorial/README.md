# Fluxor - Blazor Web Tutorials

## Redux Dev Tools

[Redux Dev Tools][ReduxDevToolsLink] is a browser plugin for
[Chrome][ChromePluginLink] and [Firefox][FirefoxPluginLink].

![](./../../../images/redux-dev-tools.jpg)

To enable Fluxor integration, follow these steps
 1. Add Fluxor.Blazor.Web.ReduxDevTools nuget package to your project.
 2. Use the `UseReduxDevTools` extension on the Fluxor options.

```c#
services.AddFluxor(o =>
	o.ScanAssemblies(typeof(SomeType).Assembly),
	o.UseReduxDevTools());
```

 3. When you run your application, click the icon for the Redux Dev Tools extension.
    Or, open the the brower's developer tools window and find the Redux tab.


### Additional options

When calling `UseReduxTools` it is possible to pass additional options to the
Redux Dev Tools browser plugin.

 * Name: The name to display.
 * Latency: How often actions are added to the plugin.
 * MaximumHistoryLength: How many actions at most should be displayed in the plugin.

 [ReduxDevToolsLink]: https://github.com/zalmoxisus/redux-devtools-extension
 [ChromePluginLink]: https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd?hl=en
 [FirefoxPluginLink]: https://addons.mozilla.org/en-GB/firefox/addon/reduxdevtools/