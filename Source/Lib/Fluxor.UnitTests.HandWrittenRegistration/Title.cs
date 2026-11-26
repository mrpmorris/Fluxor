namespace Fluxor.UnitTests.HandWrittenRegistration;

public record TitleState(string Title);

public class TitleFeature : Feature<TitleState>
{
	public override string GetName() => "Title";
	protected override TitleState GetInitialState() => new("Hello");
}
