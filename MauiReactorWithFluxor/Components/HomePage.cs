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
        base.OnMounted();
    }

    public override VisualNode Render()
        => ContentPage(
                new StoreInitializer(),
                ScrollView(
                    VStack(
                        Image("dotnet_bot.png")
                            .HeightRequest(200)
                            .HCenter()
                            .Set(MauiControls.SemanticProperties.DescriptionProperty, "Cute dot net bot waving hi to you!"),

                        Label("Hello, World!")
                            .FontSize(32)
                            .HCenter(),

                        Label("Welcome to MauiReactor: MAUI with superpowers!")
                            .FontSize(18)
                            .HCenter(),

                        Button(CounterState.Value.CurrentCount == 0 ? "Click me" : $"Clicked {CounterState.Value.CurrentCount} times!")
                            .OnClicked(() => Dispatcher.Dispatch(new CounterIncrementAction()))
                            .HCenter()
                )
                .VCenter()
                .Spacing(25)
                .Padding(30, 0)
            )
        );
}
