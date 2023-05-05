using Microsoft.CodeAnalysis;

namespace Fluxor.StoreBuilderSourceGenerator;

internal readonly record struct CompilerError
(
	string Id,
	string Title,
	Location Location = null
)
{
	public static readonly CompilerError ReducerMethodMustBeStatic = new CompilerError(
		Id: "Fluxor1",
		Title: "Reducer method must be static.");

	public static readonly CompilerError ReducerMethodMustReturnState = new CompilerError(
		Id: "Fluxor2",
		Title: "Reducer method must return state.");

	public static readonly CompilerError ReducerMethodMustHaveStateAndActionParameters = new CompilerError(
		Id: "Fluxor3",
		Title: "Reducer method must have state and action parameters.");

	public static readonly CompilerError ReducerMethodWithExplicitlyDefinedActionTypeMustHaveASingleStateParameter = new CompilerError(
		Id: "Fluxor4",
		Title: "Reducer method with explicitly defined ActionType must have a single state parameter.");

	public static readonly CompilerError ReducerMethodsReceivedStateTypeMustBeTheSameAsTheMethodsReturnType = new CompilerError(
		Id: "Fluxor5",
		Title: "Reducer method's received state type must be the same as the method's return type.");

}
