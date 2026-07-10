using Fluxor;
using Fluxor.Reactor.Maui.Components;
using Fluxor.Reactor.Maui.Middlewares.Routing;

namespace MauiReactorWithFluxor.Components;

partial class HomePage : FluxorComponent
{
    [Inject]
    private readonly IDispatcher Dispatcher;

    protected override void OnMounted()
    {
        Routing.RegisterRoute<Page2>("page-2");
        base.OnMounted();
    }

    public override VisualNode Render()
        => StoreInitializer(
            Shell(
                FlyoutItem("Page1",
                    ShellContent()
                        .RenderContent(() => ContentPage("Page1",
                            Button("Goto to Page2")
                                .HCenter()
                                .VCenter()
                            .OnClicked(()=> Dispatcher.Dispatch(new GoAction("page-2")))
                        ))
                )
            )
            .ItemTemplate(RenderItemTemplate)
        );

    static VisualNode RenderItemTemplate(MauiControls.BaseShellItem item)
        => Grid("68", "*",
            Label(item.Title)
                .VCenter()
                .Margin(10,0)
        );
}

partial class Page2 : FluxorComponent
{
    [Inject]
    private readonly IDispatcher Dispatcher;

    public override VisualNode Render()
    {
        return ContentPage("Page2",
            Button("Goto back")
                .HCenter()
                .VCenter()
            .OnClicked(()=> Dispatcher.Dispatch(new GoAction("..")))
        );
    }
}