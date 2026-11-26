using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.GeneratedRegistrationTests;

/// <summary>
/// A comparable description of what <c>AddFluxor</c> produced: the service descriptors and the store
/// built from them (features with their initial state and reducers, effects), each in order.
/// </summary>
/// <remarks>
/// Reducers and effects created from methods are identified by the method they call, written as
/// "DeclaringType.Name(ParameterTypes)" so a <see cref="MethodInfo"/> obtained through a derived type
/// (whose ReflectedType differs) compares equal to the same method bound in code.
/// The method is found by reading the wrapper's private delegate fields: the <c>Handler</c> delegate supplied
/// by generated code when set, otherwise the wrapper's own <c>Reduce</c>/<c>HandleAsync</c> delegate.
/// Compiler-generated lambdas and closures are unwrapped by following the delegate stored in the closure
/// until a delegate whose method is user code is found.
/// </remarks>
public sealed class StoreShape : IEquatable<StoreShape>
{
	// Looked up by name so this file can also be linked into projects that cannot see Fluxor's internals
	private static readonly Type ReducerWrapperType = GetFluxorType("Fluxor.DependencyInjection.Wrappers.ReducerWrapper`2");
	private static readonly Type EffectWrapperType = GetFluxorType("Fluxor.DependencyInjection.Wrappers.EffectWrapper`1");

	public IReadOnlyList<string> ServiceDescriptors { get; }
	public IReadOnlyList<string> Features { get; }
	public IReadOnlyList<string> Reducers { get; }
	public IReadOnlyList<string> Effects { get; }

	/// <summary>
	/// Every method-based reducer/effect wrapper (description as in <see cref="Reducers"/>/<see cref="Effects"/>)
	/// and whether it holds a delegate supplied by generated code (its private <c>Handler</c> field is set).
	/// Not part of equality: it tells which path built the store, not what the store is.
	/// </summary>
	public IReadOnlyList<(string Description, bool HasGeneratedHandler)> Wrappers { get; }

	private StoreShape(
		IReadOnlyList<string> serviceDescriptors,
		IReadOnlyList<string> features,
		IReadOnlyList<string> reducers,
		IReadOnlyList<string> effects,
		IReadOnlyList<(string, bool)> wrappers)
	{
		ServiceDescriptors = serviceDescriptors;
		Features = features;
		Reducers = reducers;
		Effects = effects;
		Wrappers = wrappers;
	}

	/// <summary>
	/// Captures the shape of a service collection that <c>AddFluxor</c> was called on. Builds a service
	/// provider and resolves the store inside a scope (so scoped and singleton stores both work).
	/// </summary>
	public static StoreShape Capture(IServiceCollection services)
	{
		string[] descriptors = services
			.Select(x => $"{TypeName(x.ServiceType)} -> {TypeName(x.ImplementationType)} ({x.Lifetime})")
			.ToArray();

		using ServiceProvider rootProvider = services.BuildServiceProvider();
		using IServiceScope scope = rootProvider.CreateScope();
		return Capture(scope.ServiceProvider, descriptors);
	}

	/// <summary>
	/// Captures the store shape only (no service descriptors) from a service provider.
	/// </summary>
	public static StoreShape Capture(IServiceProvider serviceProvider) =>
		Capture(serviceProvider, Array.Empty<string>());

	private static StoreShape Capture(IServiceProvider serviceProvider, string[] descriptors)
	{
		var store = (Store)serviceProvider.GetRequiredService<IStore>();

		var features = new List<string>();
		var reducers = new List<string>();
		var wrappers = new List<(string, bool)>();
		foreach (IFeature feature in store.Features.Values)
		{
			features.Add(
				$"{feature.GetName()}: {TypeName(feature.GetStateType())} = {feature.GetState()}"
				+ $" (max {feature.MaximumStateChangedNotificationsPerSecond}/s, browsable {feature.DebuggerBrowsable})");
			foreach (object reducer in GetReducers(feature))
				reducers.Add($"{feature.GetName()}: {DescribeReducerOrEffect(reducer, "Reduce", wrappers)}");
		}

		var effects = GetPrivateField<IEnumerable>(store, "Effects")
			.Cast<IEffect>()
			.Select(x => $"{DescribeReducerOrEffect(x, "HandleAsync", wrappers)} on {TypeName(GetEffectActionType(x))}")
			.ToArray();

		return new StoreShape(descriptors, features, reducers, effects, wrappers);
	}

	public static void AssertEqual(StoreShape expected, StoreShape actual)
	{
		if (!expected.Equals(actual))
			Assert.Fail("Store shapes differ:" + Environment.NewLine + Diff(expected, actual));
	}

	public bool Equals(StoreShape other) =>
		other is not null
		&& ServiceDescriptors.SequenceEqual(other.ServiceDescriptors)
		&& Features.SequenceEqual(other.Features)
		&& Reducers.SequenceEqual(other.Reducers)
		&& Effects.SequenceEqual(other.Effects);

	public override bool Equals(object obj) => Equals(obj as StoreShape);

	public override int GetHashCode() => HashCode.Combine(
		ServiceDescriptors.Count,
		Features.Count,
		Reducers.Count,
		Effects.Count);

	public override string ToString()
	{
		var builder = new StringBuilder();
		AppendSection(builder, nameof(ServiceDescriptors), ServiceDescriptors);
		AppendSection(builder, nameof(Features), Features);
		AppendSection(builder, nameof(Reducers), Reducers);
		AppendSection(builder, nameof(Effects), Effects);
		return builder.ToString();
	}

	public static string Diff(StoreShape expected, StoreShape actual)
	{
		var builder = new StringBuilder();
		DiffSection(builder, nameof(ServiceDescriptors), expected.ServiceDescriptors, actual.ServiceDescriptors);
		DiffSection(builder, nameof(Features), expected.Features, actual.Features);
		DiffSection(builder, nameof(Reducers), expected.Reducers, actual.Reducers);
		DiffSection(builder, nameof(Effects), expected.Effects, actual.Effects);
		return builder.ToString();
	}

	private static void AppendSection(StringBuilder builder, string name, IReadOnlyList<string> lines)
	{
		builder.AppendLine($"{name} ({lines.Count}):");
		foreach (string line in lines)
			builder.AppendLine("  " + line);
	}

	private static void DiffSection(
		StringBuilder builder,
		string name,
		IReadOnlyList<string> expected,
		IReadOnlyList<string> actual)
	{
		if (expected.SequenceEqual(actual))
			return;

		builder.AppendLine($"{name}: expected {expected.Count}, actual {actual.Count}");
		int count = Math.Max(expected.Count, actual.Count);
		for (int i = 0; i < count; i++)
		{
			string e = i < expected.Count ? expected[i] : "<none>";
			string a = i < actual.Count ? actual[i] : "<none>";
			if (e == a)
				builder.AppendLine($"    [{i}] {e}");
			else
			{
				builder.AppendLine($"  - [{i}] {e}");
				builder.AppendLine($"  + [{i}] {a}");
			}
		}
	}

	private static IEnumerable<object> GetReducers(IFeature feature)
	{
		for (Type type = feature.GetType(); type is not null; type = type.BaseType)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Feature<>))
			{
				FieldInfo field = type.GetField("Reducers", BindingFlags.Instance | BindingFlags.NonPublic);
				return ((IEnumerable)field.GetValue(feature)).Cast<object>().ToArray();
			}
		}
		return Array.Empty<object>();
	}

	private static string DescribeReducerOrEffect(
		object instance,
		string wrapperDelegateFieldName,
		List<(string, bool)> wrappers)
	{
		Type type = instance.GetType();
		bool isWrapper =
			type.IsGenericType
			&& (type.GetGenericTypeDefinition() == ReducerWrapperType
				|| type.GetGenericTypeDefinition() == EffectWrapperType);
		if (!isWrapper)
			return TypeName(type);

		Delegate generatedHandler = GetPrivateField<Delegate>(instance, "Handler");
		Delegate handler = generatedHandler ?? GetPrivateField<Delegate>(instance, wrapperDelegateFieldName);
		MethodInfo method = UnwrapToUserMethod(handler);
		string description = $"{TypeName(type)} calls {MethodName(method)}";
		wrappers.Add((description, generatedHandler is not null));
		return description;
	}

	private static MethodInfo UnwrapToUserMethod(Delegate handler)
	{
		var visited = new HashSet<Delegate>();
		while (handler is not null && visited.Add(handler))
		{
			if (!IsCompilerGenerated(handler.Method))
				return handler.Method;

			// A lambda: follow the delegate captured in its closure
			object target = handler.Target;
			handler = target?.GetType()
				.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Select(x => x.GetValue(target))
				.OfType<Delegate>()
				.FirstOrDefault();
		}
		throw new InvalidOperationException("Could not find the method called by a wrapper");
	}

	private static bool IsCompilerGenerated(MethodInfo method) =>
		method.IsDefined(typeof(CompilerGeneratedAttribute), false)
		|| method.DeclaringType?.IsDefined(typeof(CompilerGeneratedAttribute), false) == true
		|| method.Name.Contains('<');

	private static Type GetEffectActionType(IEffect effect)
	{
		for (Type type = effect.GetType(); type is not null; type = type.BaseType)
		{
			if (type.IsGenericType
				&& (type.GetGenericTypeDefinition() == EffectWrapperType
					|| type.GetGenericTypeDefinition() == typeof(Effect<>)))
				return type.GetGenericArguments()[0];
		}
		return null;
	}

	private static Type GetFluxorType(string fullName) =>
		typeof(IStore).Assembly.GetType(fullName, throwOnError: true);

	private static T GetPrivateField<T>(object instance, string name) where T : class
	{
		for (Type type = instance.GetType(); type is not null; type = type.BaseType)
		{
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			if (field is not null)
				return (T)field.GetValue(instance);
		}
		throw new InvalidOperationException($"Field {name} not found on {instance.GetType()}");
	}

	private static string MethodName(MethodInfo method) =>
		$"{TypeName(method.DeclaringType)}.{method.Name}"
		+ $"({string.Join(", ", method.GetParameters().Select(x => TypeName(x.ParameterType)))})"
		+ (method.IsStatic ? " static" : "");

	private static string TypeName(Type type)
	{
		if (type is null)
			return "null";
		if (!type.IsGenericType)
			return type.FullName ?? type.Name;
		string name = type.GetGenericTypeDefinition().FullName;
		name = name.Substring(0, name.IndexOf('`'));
		return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(TypeName))}>";
	}
}
