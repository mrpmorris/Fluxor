using Fluxor;
using Fluxor.Reactor.Maui;
using Fluxor.Reactor.Maui.Components;

namespace MauiReactorWithFluxor.Components;

partial class HomePage : FluxorComponent
{
    [Inject]
    IDispatcher Dispatcher;

    [Inject]
    IState<CounterState> CounterState;

    protected override void OnMounted()
    {
        Routing.RegisterRoute<Page2>("page-2");
        base.OnMounted();
    }

    public override VisualNode Render()
        => Shell(
            FlyoutItem("Page1",
                ShellContent()
                    .RenderContent(() => ContentPage("Page1",
                        Button("Goto to Page2")
                            .HCenter()
                            .VCenter()
                        .OnClicked(async ()=> await MauiControls.Shell.Current.GoToAsync("page-2"))
                    ))
            )
        )
        .ItemTemplate(RenderItemTemplate);            

    static VisualNode RenderItemTemplate(MauiControls.BaseShellItem item)
        => Grid("68", "*",
            Label(item.Title)
                .VCenter()
                .Margin(10,0)
        );
}

class Page2 : Component
{
    public override VisualNode Render()
    {
        return ContentPage("Page2",
            Button("Goto back")
                .HCenter()
                .VCenter()
            .OnClicked(async ()=> await MauiControls.Shell.Current.GoToAsync(".."))
        );
    }
}