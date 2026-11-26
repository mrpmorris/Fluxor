using System.Collections;
using System.Reflection;
using Fluxor.DependencyInjection.Generated;

namespace Fluxor.Performance.BlazorWasm;

/// <summary>
/// The store an app built, so a variant cannot look faster by registering less. Method reducers/effects are the
/// ones made from <c>[ReducerMethod]</c>/<c>[EffectMethod]</c> (Fluxor's wrappers); generated ones are those of
/// them that generated code created (their wrapper holds the generated delegate), so a generated build that fell
/// back to the reflection scan shows 0. Also linked into Fluxor.PerformanceTests.
/// </summary>
public sealed record StoreShape(
	int Features,
	int Reducers,
	int Effects,
	int MethodReducers,
	int MethodEffects,
	int GeneratedReducers,
	int GeneratedEffects,
	bool HasGeneratedRegistration)
{
	/// <summary>Reads the shape of an initialized store. Uses reflection: call it only after measuring.</summary>
	public static StoreShape Read(IStore store, Assembly sampleAssembly)
	{
		object[] reducers = store.Features.Values
			.SelectMany(x => GetField<IEnumerable>(x, "Reducers").Cast<object>())
			.ToArray();
		object[] effects = GetField<IEnumerable>(store, "Effects").Cast<object>().ToArray();
		return new StoreShape(
			Features: store.Features.Count,
			Reducers: reducers.Length,
			Effects: effects.Length,
			MethodReducers: reducers.Count(IsMethodWrapper),
			MethodEffects: effects.Count(IsMethodWrapper),
			GeneratedReducers: reducers.Count(HasGeneratedHandler),
			GeneratedEffects: effects.Count(HasGeneratedHandler),
			HasGeneratedRegistration: sampleAssembly.GetCustomAttribute<FluxorGeneratedRegistrationAttribute>() is not null);
	}

	private static bool IsMethodWrapper(object wrapper) => GetHandlerField(wrapper) is not null;

	private static bool HasGeneratedHandler(object wrapper) => GetHandlerField(wrapper)?.GetValue(wrapper) is not null;

	// Fluxor's reducer and effect method wrappers keep the delegate generated code supplied in Handler
	private static FieldInfo? GetHandlerField(object wrapper) =>
		wrapper.GetType().GetField("Handler", BindingFlags.Instance | BindingFlags.NonPublic);

	// Fluxor keeps reducers and effects in non-public lists
	private static T GetField<T>(object instance, string name)
	{
		for (Type? type = instance.GetType(); type is not null; type = type.BaseType)
		{
			FieldInfo? field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			if (field is not null)
				return (T)field.GetValue(instance)!;
		}
		throw new MissingFieldException(instance.GetType().FullName, name);
	}
}
