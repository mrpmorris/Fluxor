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
<ItemGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
    <PackageReference Include="Fluxor.Blazor.Web.ReduxDevTools\Fluxor.Blazor.Web.ReduxDevTools" Version="....." />
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

### Controlling JSON options

It is possible to specify which JSON library the Redux Dev Tools Middleware
plugin should use when serializing to / deserializing from the
Redux Dev Tools browser plugin.

By default, `System.Text.Json` and `Newtonsoft.Json` are supported. In both cases,
the action that creates the Options/Settings is passed an `IServiceProvider`.

The following example shows how to use `System.Text.Json` as the
JSON serialization library, and how to specify that you want
it to use `camelCase` property names in the generated JSON.

```c#
services.AddFluxor(o =>
  o.ScanAssemblies(typeof(SomeType).Assembly),
  o.UseReduxDevTools(rdt =>
    {
      rdt.UseSystemTextJson(_ =>
        new System.Text.Json.JsonSerializerOptions
        {
          PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        }
      );
    }
  )
);
```

The following example shows how to use `Newtonsoft.Json` to achieve
the same goal.

```c#
services.AddFluxor(o =>
  o.ScanAssemblies(typeof(SomeType).Assembly),
  o.UseReduxDevTools(rdt =>
    {
      rdt.UseNewtonsoftJson(_ =>
        new Newtonsoft.Json.JsonSerializerSettings
        {
          ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
          {
            NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
          }
        }
      );
    }
  )
);
```

#### Other JSON libraries

Although only `System.Text.Json` and `Newtonsoft.Json` are supported by default, it
is possible to have the Redux Dev Tools middleware use any library of your choice.

This is achieved by creating a class that implements
`Fluxor.Blazor.Web.ReduxDevTools.IJsonSerialization`, and then registering it as
the implementor of that interface udring the Dependency Injection phase.

```c#
// JSON library adapter class
public class MyJsonLibAdapter : IJsonSerialization
{
  public object Deserialize(string json, Type type) =>
    MyJsonLib.DeserializeObject(json, type);

  public string Serialize(object source, Type type) =>
    MyJsonLib.SerializeObject(source, type);  
}
```

```c#
// Service bootstrapper  
services.AddFluxor(...etc...);
services.AddScoped<MyJsonLibAdapter, IJsonSerialization>();
```

 [ReduxDevToolsLink]: https://github.com/zalmoxisus/redux-devtools-extension
 [ChromePluginLink]: https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd?hl=en
 [FirefoxPluginLink]: https://addons.mozilla.org/en-GB/firefox/addon/reduxdevtools/
 [FluxorReduxDevToolsLink]: https://www.nuget.org/packages/Fluxor.Blazor.Web.ReduxDevTools/
