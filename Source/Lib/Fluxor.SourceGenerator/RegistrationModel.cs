using Microsoft.CodeAnalysis;

namespace Fluxor.SourceGenerator;

/// <summary>
/// What the generator found in one assembly: the entries to emit, or the problems that prevent emitting.
/// All type names are <c>global::</c>-qualified C#.
/// </summary>
internal sealed class RegistrationModel
{
	public RegistrationModel(string @namespace)
	{
		Namespace = @namespace;
	}

	/// <summary>The namespace of the generated class.</summary>
	public string Namespace { get; }

	/// <summary>One entry per Fluxor-relevant type. Their order does not matter: the runtime sorts them by MetadataToken.</summary>
	public List<TypeEntryModel> Entries { get; } = new();

	/// <summary>Custom DiagnosticIds of obsolete members the generated code references, to disable in the file.</summary>
	public SortedSet<string> SuppressedDiagnosticIds { get; } = new(StringComparer.Ordinal);

	/// <summary>Anything the generated code cannot reproduce exactly. When not empty, nothing is emitted.</summary>
	public List<RegistrationProblem> Problems { get; } = new();
}

/// <summary>
/// A member the generated code cannot register exactly as the reflection scan does.
/// </summary>
internal sealed class RegistrationProblem
{
	public RegistrationProblem(string member, Location location)
	{
		Member = member;
		Location = location;
	}

	/// <summary>The member's name followed by the reason, e.g. "Ns.Host.Reduce (is a generic method)".</summary>
	public string Member { get; }

	public Location Location { get; }
}

/// <summary>
/// Mirrors a GeneratedTypeEntry.
/// </summary>
internal sealed class TypeEntryModel
{
	public TypeEntryModel(string type)
	{
		Type = type;
	}

	public string Type { get; }
	public bool IsMiddleware { get; set; }
	public FeatureClassModel? FeatureClass { get; set; }
	public FeatureStateModel? FeatureState { get; set; }
	public string? ReducerClassStateType { get; set; }
	public bool IsEffectClass { get; set; }

	/// <summary>In the order <c>Type.GetMethods</c> returns them.</summary>
	public List<ReducerMethodModel> ReducerMethods { get; } = new();

	/// <summary>In the order <c>Type.GetMethods</c> returns them.</summary>
	public List<EffectMethodModel> EffectMethods { get; } = new();

	public bool HasMethods => ReducerMethods.Count > 0 || EffectMethods.Count > 0;
}

internal sealed class FeatureClassModel
{
	public FeatureClassModel(string stateType)
	{
		StateType = stateType;
	}

	public string StateType { get; }
}

internal enum InitialStateKind
{
	/// <summary><c>new S()</c></summary>
	Constructor,
	/// <summary><c>Activator.CreateInstance(typeof(S), nonPublic: true)</c>, as Fluxor's FeatureStateInfo does.</summary>
	ActivatorConstructor,
	/// <summary>The factory method bound as a method group.</summary>
	FactoryMethod,
	/// <summary>The factory method found with <c>GetMethod</c>, as Fluxor's FeatureStateInfo does.</summary>
	ReflectedFactoryMethod
}

internal sealed class FeatureStateModel
{
	public FeatureStateModel(
		string stateType,
		string? name,
		byte maximumStateChangedNotificationsPerSecond,
		bool debuggerBrowsable,
		InitialStateKind initialStateKind,
		string? factoryMethodName)
	{
		StateType = stateType;
		Name = name;
		MaximumStateChangedNotificationsPerSecond = maximumStateChangedNotificationsPerSecond;
		DebuggerBrowsable = debuggerBrowsable;
		InitialStateKind = initialStateKind;
		FactoryMethodName = factoryMethodName;
	}

	public string StateType { get; }
	public string? Name { get; }
	public byte MaximumStateChangedNotificationsPerSecond { get; }
	public bool DebuggerBrowsable { get; }
	public InitialStateKind InitialStateKind { get; }

	/// <summary>The method name as written in metadata (for GetMethod) or C# (for a method group).</summary>
	public string? FactoryMethodName { get; }
}

internal sealed class ReducerMethodModel
{
	public ReducerMethodModel(string stateType, string actionType, bool isStatic, bool takesAction, string methodGroup)
	{
		StateType = stateType;
		ActionType = actionType;
		IsStatic = isStatic;
		TakesAction = takesAction;
		MethodGroup = methodGroup;
	}

	public string StateType { get; }
	public string ActionType { get; }
	public bool IsStatic { get; }
	public bool TakesAction { get; }

	/// <summary>The C# method group, e.g. <c>global::Ns.Host.Reduce</c> or <c>((global::Ns.Host)host).Reduce</c>.</summary>
	public string MethodGroup { get; }
}

internal sealed class EffectMethodModel
{
	public EffectMethodModel(string actionType, bool isStatic, bool takesAction, string methodGroup)
	{
		ActionType = actionType;
		IsStatic = isStatic;
		TakesAction = takesAction;
		MethodGroup = methodGroup;
	}

	public string ActionType { get; }
	public bool IsStatic { get; }
	public bool TakesAction { get; }

	/// <summary>The C# method group, e.g. <c>global::Ns.Host.Handle</c> or <c>((global::Ns.Host)host).Handle</c>.</summary>
	public string MethodGroup { get; }
}
