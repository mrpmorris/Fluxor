using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Text;

namespace Fluxor.SourceGenerator;

/// <summary>
/// Finds, at build time, what Fluxor's reflection scan (<c>ReflectionScanner</c>, the <c>InfoFactories</c>,
/// <c>AssemblyScanSettings.FilterMethods</c> and <c>TypeHelper</c>) finds in one assembly:
/// <list type="bullet">
///   <item>Candidate types: every type declared in the assembly except generic type definitions
///   (a type nested in a generic type is one too).</item>
///   <item>Middleware: any type implementing <c>IMiddleware</c>, generic definitions included.</item>
///   <item>Feature, reducer and effect classes: non-abstract types only; the state type of the first
///   implemented <c>IFeature&lt;T&gt;</c> / <c>IReducer&lt;T&gt;</c>.</item>
///   <item><c>[FeatureState]</c>: on any candidate type, abstract ones too, attribute declared on the type.</item>
///   <item>Methods: <c>GetMethods(Public | Instance | Static | FlattenHierarchy)</c> carrying
///   <c>[ReducerMethod]</c> / <c>[EffectMethod]</c> on the method itself (the most derived override).</item>
/// </list>
/// Anything the generated code cannot reproduce exactly, and anything the reflection scan rejects, becomes a
/// <see cref="RegistrationProblem"/>; nothing is approximated.
/// </summary>
internal sealed class RegistrationModelBuilder
{
	private readonly Compilation Compilation;
	private readonly FluxorSymbols Symbols;
	private readonly RegistrationModel Model;

	private RegistrationModelBuilder(Compilation compilation, FluxorSymbols symbols)
	{
		Compilation = compilation;
		Symbols = symbols;
		Model = new RegistrationModel(GetNamespace(compilation.AssemblyName));
	}

	public static RegistrationModel Build(
		Compilation compilation,
		FluxorSymbols symbols,
		ImmutableArray<AdditionalText> razorFiles,
		CancellationToken cancellationToken)
	{
		var builder = new RegistrationModelBuilder(compilation, symbols);
		builder.AddRazorFileProblems(razorFiles, cancellationToken);
		foreach (INamedTypeSymbol type in GetDeclaredTypes(compilation.Assembly.GlobalNamespace))
		{
			cancellationToken.ThrowIfCancellationRequested();
			builder.AddType(type);
		}
		return builder.Model;
	}

	public static bool IsRazorFile(AdditionalText file) =>
		file.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)
		|| file.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Types written in Razor files are produced by the Razor source generator, which this generator cannot see.
	/// See <see cref="RazorFileGuard"/>.
	/// </summary>
	private void AddRazorFileProblems(ImmutableArray<AdditionalText> razorFiles, CancellationToken cancellationToken)
	{
		foreach (RegistrationProblem problem in RazorFileGuard.GetProblems(Compilation, Symbols, razorFiles, cancellationToken))
			Model.Problems.Add(problem);
	}

	private void AddType(INamedTypeSymbol type)
	{
		bool isMiddleware = FluxorSymbols.Implements(type, Symbols.MiddlewareInterface);
		if (IsGenericTypeDefinition(type))
		{
			// The scan takes the namespace of every IMiddleware type, generic or not, as a middleware location,
			// but generated entries cannot describe a generic type definition
			if (isMiddleware)
				AddProblem(type, type.ToDisplayString() + " (a generic type definition implementing IMiddleware)");
			return;
		}

		// Static classes and interfaces are abstract in metadata
		bool isAbstract = type.IsAbstract || type.IsStatic;
		var entry = new TypeEntryModel(Display(type)) { IsMiddleware = isMiddleware };

		if (!isAbstract)
		{
			entry.FeatureClass = GetFeatureClass(type);
			entry.ReducerClassStateType = GetReducerClassStateType(type);
			entry.IsEffectClass = FluxorSymbols.Implements(type, Symbols.EffectInterface);
		}
		entry.FeatureState = GetFeatureState(type);
		AddMethods(type, isAbstract, entry);

		bool hasTypeRole =
			entry.IsMiddleware
			|| entry.FeatureClass is not null
			|| entry.FeatureState is not null
			|| entry.ReducerClassStateType is not null
			|| entry.IsEffectClass;
		if (!hasTypeRole && !entry.HasMethods)
			return;

		// Methods already report an inaccessible host themselves
		string? typeProblem = GetTypeProblem(type);
		if (typeProblem is not null)
		{
			if (hasTypeRole)
				AddProblem(type, type.ToDisplayString() + " (" + typeProblem + ")");
			return;
		}

		Model.Entries.Add(entry);
	}

	private FeatureClassModel? GetFeatureClass(INamedTypeSymbol type)
	{
		ITypeSymbol? stateType = GetSingleImplementedTypeArgument(type, Symbols.FeatureInterface, "IFeature<TState>");
		return stateType is null ? null : new FeatureClassModel(Display(stateType));
	}

	private string? GetReducerClassStateType(INamedTypeSymbol type)
	{
		ITypeSymbol? stateType = GetSingleImplementedTypeArgument(type, Symbols.ReducerInterface, "IReducer<TState>");
		return stateType is null ? null : Display(stateType);
	}

	// TypeHelper takes the first match in Type.GetInterfaces(), whose order is not defined
	private ITypeSymbol? GetSingleImplementedTypeArgument(INamedTypeSymbol type, INamedTypeSymbol genericInterface, string interfaceName)
	{
		List<ITypeSymbol> stateTypes = FluxorSymbols.GetImplementedTypeArguments(type, genericInterface).ToList();
		if (stateTypes.Count == 0)
			return null;

		if (stateTypes.Count > 1)
		{
			AddProblem(type, type.ToDisplayString() + " (implements " + interfaceName + " more than once)");
			return null;
		}

		string? problem = GetTypeProblem(stateTypes[0]);
		if (problem is not null)
		{
			AddProblem(type, type.ToDisplayString() + " (state type " + stateTypes[0].ToDisplayString() + " " + problem + ")");
			return null;
		}
		return stateTypes[0];
	}

	private FeatureStateModel? GetFeatureState(INamedTypeSymbol type)
	{
		AttributeData? attribute = null;
		for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
		{
			foreach (AttributeData candidate in current.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(candidate.AttributeClass, Symbols.FeatureStateAttribute))
				{
					// FeatureStateAttribute is not inherited
					if (SymbolEqualityComparer.Default.Equals(current, type))
						attribute = candidate;
				}
				else if (FluxorSymbols.IsOrDerivesFrom(candidate.AttributeClass, Symbols.FeatureStateAttribute))
				{
					AddProblem(type, type.ToDisplayString() + " (uses an attribute derived from FeatureStateAttribute)");
					return null;
				}
			}
		}

		if (attribute is null)
			return null;

		string? name = null;
		string? factoryMethodName = null;
		bool debuggerBrowsable = true;
		byte maximumStateChangedNotificationsPerSecond = 0;
		foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
		{
			object? value = argument.Value.Value;
			switch (argument.Key)
			{
				case "Name":
					name = value as string;
					break;
				case "CreateInitialStateMethodName":
					factoryMethodName = value as string;
					break;
				case "DebuggerBrowsable" when value is bool browsable:
					debuggerBrowsable = browsable;
					break;
				case "MaximumStateChangedNotificationsPerSecond" when value is byte maximum:
					maximumStateChangedNotificationsPerSecond = maximum;
					break;
				default:
					AddProblem(type, type.ToDisplayString() + " (sets FeatureStateAttribute." + argument.Key + " in a way generated code does not understand)");
					return null;
			}
		}

		string display = Display(type);
		if (factoryMethodName is not null)
		{
			// FeatureStateInfo: GetMethod(name, Static | Public | NonPublic), so declared static methods only;
			// several of them throw AmbiguousMatchException
			List<IMethodSymbol> methods = type.GetMembers(factoryMethodName)
				.OfType<IMethodSymbol>()
				.Where(x => x.IsStatic)
				.ToList();

			string describedMethod = type.ToDisplayString() + "." + factoryMethodName;
			if (methods.Count != 1)
			{
				AddProblem(type, describedMethod + " (CreateInitialStateMethodName must name exactly one static method of the state)");
				return null;
			}

			IMethodSymbol method = methods[0];
			if (method.Parameters.Length != 0
				|| method.IsGenericMethod
				|| method.ReturnsByRef
				|| method.ReturnsByRefReadonly
				|| !SymbolEqualityComparer.Default.Equals(method.ReturnType, type))
			{
				AddProblem(method, describedMethod + " (must be a parameterless method returning " + type.ToDisplayString() + ")");
				return null;
			}

			bool canBind =
				method.MethodKind == MethodKind.Ordinary
				&& IsAccessibleFromGeneratedCode(method)
				&& !IsUnusable(method)
				&& SyntaxFacts.IsValidIdentifier(method.Name);
			return new FeatureStateModel(
				display,
				name,
				maximumStateChangedNotificationsPerSecond,
				debuggerBrowsable,
				canBind ? InitialStateKind.FactoryMethod : InitialStateKind.ReflectedFactoryMethod,
				canBind ? display + "." + EscapeIdentifier(method.Name) : factoryMethodName);
		}

		// FeatureStateInfo: GetConstructor(Instance | Public | NonPublic, no parameters), then Activator
		IMethodSymbol? constructor = type.IsStatic
			? null
			: type.InstanceConstructors.FirstOrDefault(x => x.Parameters.Length == 0);
		if (constructor is null)
		{
			AddProblem(type, type.ToDisplayString() + " (a [FeatureState] needs a parameterless constructor or a CreateInitialStateMethodName)");
			return null;
		}

		bool canUseNew =
			!type.IsAbstract
			&& IsAccessibleFromGeneratedCode(constructor)
			&& !IsUnusable(constructor)
			&& !HasRequiredMembers(type);
		return new FeatureStateModel(
			display,
			name,
			maximumStateChangedNotificationsPerSecond,
			debuggerBrowsable,
			canUseNew ? InitialStateKind.Constructor : InitialStateKind.ActivatorConstructor,
			factoryMethodName: null);
	}

	private void AddMethods(INamedTypeSymbol host, bool hostIsAbstract, TypeEntryModel entry)
	{
		foreach (IMethodSymbol method in GetScannedMethods(host))
		{
			AttributeData? reducerAttribute = FindMethodAttribute(host, method, Symbols.ReducerMethodAttribute, "ReducerMethodAttribute");
			AttributeData? effectAttribute = FindMethodAttribute(host, method, Symbols.EffectMethodAttribute, "EffectMethodAttribute");
			if (reducerAttribute is null && effectAttribute is null)
				continue;

			string describedMethod = host.ToDisplayString() + "." + method.Name;
			string? callProblem = GetCallProblem(host, hostIsAbstract, method);
			if (callProblem is not null)
			{
				AddProblem(method, host, describedMethod + " (" + callProblem + ")");
				continue;
			}

			if (reducerAttribute is not null)
			{
				ReducerMethodModel? reducer = CreateReducerMethod(host, method, reducerAttribute, describedMethod);
				if (reducer is not null)
					entry.ReducerMethods.Add(reducer);
			}

			if (effectAttribute is not null)
			{
				EffectMethodModel? effect = CreateEffectMethod(host, method, effectAttribute, describedMethod);
				if (effect is not null)
					entry.EffectMethods.Add(effect);
			}
		}
	}

	/// <summary>
	/// What <c>GetMethods(Public | Instance | Static | FlattenHierarchy)</c> returns: the type's own public
	/// methods, then those of each base class (public static ones included), leaving out base methods that a
	/// more derived class overrides. Methods hidden with <c>new</c> are still returned. An interface returns
	/// only its own methods.
	/// </summary>
	internal static IEnumerable<IMethodSymbol> GetScannedMethods(INamedTypeSymbol type)
	{
		var overridden = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
		INamedTypeSymbol? current = type;
		while (current is not null)
		{
			foreach (IMethodSymbol method in current.GetMembers().OfType<IMethodSymbol>())
			{
				if (method.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor)
					continue;

				for (IMethodSymbol? baseMethod = method.OverriddenMethod; baseMethod is not null; baseMethod = baseMethod.OverriddenMethod)
					overridden.Add(baseMethod.OriginalDefinition);

				if (method.DeclaredAccessibility != Accessibility.Public)
					continue;
				if (overridden.Contains(method.OriginalDefinition))
					continue;
				yield return method;
			}
			current = type.TypeKind == TypeKind.Interface ? null : current.BaseType;
		}
	}

	/// <summary>
	/// The Fluxor attribute declared on the method itself. The scan's own factories read it with
	/// <c>GetCustomAttribute&lt;T&gt;(inherit: false)</c>, so an attribute only on an overridden base method
	/// registers nothing.
	/// </summary>
	private AttributeData? FindMethodAttribute(INamedTypeSymbol host, IMethodSymbol method, INamedTypeSymbol attributeType, string attributeName)
	{
		AttributeData? result = null;
		foreach (AttributeData attribute in method.GetAttributes())
		{
			if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType))
				result = attribute;
			else if (FluxorSymbols.IsOrDerivesFrom(attribute.AttributeClass, attributeType))
				AddProblem(method, host, host.ToDisplayString() + "." + method.Name + " (uses an attribute derived from " + attributeName + ")");
		}
		return result;
	}

	/// <summary>Null when generated code can bind a delegate to the method exactly as the scan does.</summary>
	private string? GetCallProblem(INamedTypeSymbol host, bool hostIsAbstract, IMethodSymbol method)
	{
		if (method.MethodKind != MethodKind.Ordinary || !SyntaxFacts.IsValidIdentifier(method.Name))
			return "is not an ordinary method";
		if (method.IsGenericMethod)
			return "is a generic method";
		if (method.IsStatic && (method.IsAbstract || method.IsVirtual))
			return "is a static abstract or virtual interface member";
		if (!method.IsStatic && hostIsAbstract)
			return "is an instance method on an abstract type, which the scan has no instance of";
		if (!method.IsStatic && host.IsValueType)
			return "is an instance method on a struct";
		if (method.ReturnsByRef || method.ReturnsByRefReadonly || method.Parameters.Any(x => x.RefKind != RefKind.None))
			return "has ref, in or out parameters or returns by reference";
		if (IsUnusable(method))
			return "is obsolete as an error or experimental";

		string? hostProblem = GetTypeProblem(host);
		if (hostProblem is not null)
			return "is declared on a type that " + hostProblem;

		string? declaringTypeProblem = GetTypeProblem(method.ContainingType);
		if (declaringTypeProblem is not null)
			return "is declared on " + method.ContainingType.ToDisplayString() + ", which " + declaringTypeProblem;

		return null;
	}

	// Mirrors ReducerMethodInfo's checks
	private ReducerMethodModel? CreateReducerMethod(INamedTypeSymbol host, IMethodSymbol method, AttributeData attribute, string describedMethod)
	{
		if (!TryGetAttributeActionType(attribute, out ITypeSymbol? attributeActionType))
		{
			AddProblem(method, host, describedMethod + " (has a null action type)");
			return null;
		}

		int expectedParameters = attributeActionType is null ? 2 : 1;
		if (method.Parameters.Length != expectedParameters)
		{
			AddProblem(method, host, describedMethod + (attributeActionType is null
				? " (must have 2 parameters (state, action))"
				: " (must have 1 parameter (state) when the action type is in the attribute)"));
			return null;
		}

		ITypeSymbol stateType = method.Parameters[0].Type;
		if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, stateType))
		{
			AddProblem(method, host, describedMethod + " (must return its state parameter's type)");
			return null;
		}

		ITypeSymbol actionType = attributeActionType ?? method.Parameters[1].Type;
		if (!CheckMessageTypes(method, host, describedMethod, stateType, actionType))
			return null;

		return new ReducerMethodModel(
			stateType: Display(stateType),
			actionType: Display(actionType),
			isStatic: method.IsStatic,
			takesAction: attributeActionType is null,
			methodGroup: GetMethodGroup(method));
	}

	// Mirrors EffectMethodInfo's checks
	private EffectMethodModel? CreateEffectMethod(INamedTypeSymbol host, IMethodSymbol method, AttributeData attribute, string describedMethod)
	{
		if (!TryGetAttributeActionType(attribute, out ITypeSymbol? attributeActionType))
		{
			AddProblem(method, host, describedMethod + " (has a null action type)");
			return null;
		}

		int expectedParameters = attributeActionType is null ? 2 : 1;
		if (method.Parameters.Length != expectedParameters)
		{
			AddProblem(method, host, describedMethod + (attributeActionType is null
				? " (must have 2 parameters (action, IDispatcher))"
				: " (must have 1 parameter (IDispatcher) when the action type is in the attribute)"));
			return null;
		}

		if (!SymbolEqualityComparer.Default.Equals(method.Parameters[method.Parameters.Length - 1].Type, Symbols.Dispatcher))
		{
			AddProblem(method, host, describedMethod + " (its last parameter must be an IDispatcher)");
			return null;
		}

		if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, Symbols.Task))
		{
			AddProblem(method, host, describedMethod + " (must return Task)");
			return null;
		}

		ITypeSymbol actionType = attributeActionType ?? method.Parameters[0].Type;
		if (!CheckMessageTypes(method, host, describedMethod, stateType: null, actionType))
			return null;

		return new EffectMethodModel(
			actionType: Display(actionType),
			isStatic: method.IsStatic,
			takesAction: attributeActionType is null,
			methodGroup: GetMethodGroup(method));
	}

	private bool CheckMessageTypes(IMethodSymbol method, INamedTypeSymbol host, string describedMethod, ITypeSymbol? stateType, ITypeSymbol actionType)
	{
		if (stateType is not null)
		{
			string? stateProblem = GetTypeArgumentProblem(stateType);
			if (stateProblem is not null)
			{
				AddProblem(method, host, describedMethod + " (state type " + stateType.ToDisplayString() + " " + stateProblem + ")");
				return false;
			}
		}

		string? actionProblem = GetTypeArgumentProblem(actionType);
		if (actionProblem is not null)
		{
			AddProblem(method, host, describedMethod + " (action type " + actionType.ToDisplayString() + " " + actionProblem + ")");
			return false;
		}
		return true;
	}

	/// <summary>
	/// The action type given to the attribute's constructor, or null for the parameterless constructor.
	/// False when the attribute was given null (its constructor throws, so the scan fails).
	/// </summary>
	private static bool TryGetAttributeActionType(AttributeData attribute, out ITypeSymbol? actionType)
	{
		actionType = null;
		if (attribute.ConstructorArguments.Length == 0)
			return true;

		actionType = attribute.ConstructorArguments[0].Value as ITypeSymbol;
		return actionType is not null;
	}

	/// <summary>
	/// The method group, qualified with the type that declares the method so that a method hidden with
	/// <c>new</c> still binds to itself. Instance methods are bound on the host instance the scan resolves.
	/// </summary>
	private static string GetMethodGroup(IMethodSymbol method)
	{
		string declaringType = Display(method.ContainingType);
		string name = EscapeIdentifier(method.Name);
		return method.IsStatic
			? declaringType + "." + name
			: "((" + declaringType + ")host)." + name;
	}

	/// <summary>A state or action type must also be usable as a generic type argument.</summary>
	private string? GetTypeArgumentProblem(ITypeSymbol type)
	{
		if (type.IsStatic)
			return "is a static class";
		if (type.SpecialType == SpecialType.System_Void)
			return "is void";
		return GetTypeProblem(type);
	}

	/// <summary>Null when generated code in this assembly can write <c>typeof(type)</c> exactly.</summary>
	private string? GetTypeProblem(ITypeSymbol type)
	{
		switch (type)
		{
			case IArrayTypeSymbol array:
				return GetTypeProblem(array.ElementType);

			case INamedTypeSymbol named:
				if (named.TypeKind == TypeKind.Error)
					return "does not compile";
				if (named.IsTupleType)
					return "is a tuple type";
				if (named.IsRefLikeType)
					return "is a ref struct";
				if (named.IsUnboundGenericType)
					return "is an open generic type";

				for (INamedTypeSymbol? current = named; current is not null; current = current.ContainingType)
				{
					if (current.IsFileLocal)
						return "is file-local";
					if (!Compilation.IsSymbolAccessibleWithin(current.OriginalDefinition, Compilation.Assembly))
						return "is not accessible from generated code (private or protected)";
					if (IsUnusable(current))
						return "is obsolete as an error or experimental";
					foreach (ITypeSymbol typeArgument in current.TypeArguments)
					{
						string? problem = GetTypeArgumentProblem(typeArgument);
						if (problem is not null)
							return "has a type argument that " + problem;
					}
				}
				return null;

			case ITypeParameterSymbol:
				return "is an open type parameter";

			default:
				return "is a dynamic, pointer or function pointer type";
		}
	}

	private bool IsAccessibleFromGeneratedCode(ISymbol symbol) =>
		Compilation.IsSymbolAccessibleWithin(symbol, Compilation.Assembly);

	/// <summary>
	/// Referencing the symbol from generated code would be a compile error: obsolete with error = true, or
	/// experimental and declared in another assembly. An obsolete warning reported under a custom
	/// <c>DiagnosticId</c> is not covered by the generated file's CS0612/CS0618 pragma, so its id is added to
	/// the pragma; an id that cannot be written in a pragma makes the symbol unusable.
	/// </summary>
	private bool IsUnusable(ISymbol symbol)
	{
		foreach (AttributeData attribute in symbol.GetAttributes())
		{
			if (Symbols.ObsoleteAttribute is not null
				&& SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, Symbols.ObsoleteAttribute))
			{
				if (attribute.ConstructorArguments.Length == 2 && attribute.ConstructorArguments[1].Value is true)
					return true;

				foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
				{
					if (argument.Key != "DiagnosticId" || argument.Value.Value is not string diagnosticId)
						continue;
					if (!IsPragmaDiagnosticId(diagnosticId))
						return true;
					Model.SuppressedDiagnosticIds.Add(diagnosticId);
				}
			}

			if (Symbols.ExperimentalAttribute is not null
				&& SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, Symbols.ExperimentalAttribute)
				&& !SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, Compilation.Assembly))
				return true;
		}
		return false;
	}

	private static bool IsPragmaDiagnosticId(string diagnosticId) =>
		diagnosticId.Length > 0 && diagnosticId.All(x => x == '_' || (x < 128 && char.IsLetterOrDigit(x)));

	/// <summary><c>new T()</c> does not compile for a type with required members; Activator ignores them.</summary>
	private static bool HasRequiredMembers(INamedTypeSymbol type)
	{
		for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
		{
			foreach (ISymbol member in current.GetMembers())
			{
				if (member is IPropertySymbol { IsRequired: true } || member is IFieldSymbol { IsRequired: true })
					return true;
			}
		}
		return false;
	}

	private static bool IsGenericTypeDefinition(INamedTypeSymbol type)
	{
		for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
			if (current.TypeParameters.Length > 0)
				return true;
		return false;
	}

	// The order of the entries does not matter: GeneratedRegistrationCatalog sorts them by MetadataToken,
	// which is the order Assembly.GetTypes() returns types in
	internal static IEnumerable<INamedTypeSymbol> GetDeclaredTypes(INamespaceSymbol @namespace)
	{
		foreach (INamespaceOrTypeSymbol member in @namespace.GetMembers())
		{
			if (member is INamespaceSymbol childNamespace)
			{
				foreach (INamedTypeSymbol type in GetDeclaredTypes(childNamespace))
					yield return type;
			}
			else if (member is INamedTypeSymbol type)
			{
				foreach (INamedTypeSymbol nestedOrSelf in GetTypeAndNestedTypes(type))
					yield return nestedOrSelf;
			}
		}
	}

	private static IEnumerable<INamedTypeSymbol> GetTypeAndNestedTypes(INamedTypeSymbol type)
	{
		yield return type;
		foreach (INamedTypeSymbol nested in type.GetTypeMembers())
			foreach (INamedTypeSymbol result in GetTypeAndNestedTypes(nested))
				yield return result;
	}

	private void AddProblem(ISymbol symbol, string member) =>
		Model.Problems.Add(new RegistrationProblem(member, GetLocation(symbol)));

	// Inherited methods may come from metadata; report them on the type that inherits them
	private void AddProblem(IMethodSymbol method, INamedTypeSymbol host, string member)
	{
		Location location = GetLocation(method);
		Model.Problems.Add(new RegistrationProblem(member, location == Location.None ? GetLocation(host) : location));
	}

	private static Location GetLocation(ISymbol symbol) =>
		symbol.Locations.FirstOrDefault(x => x.IsInSource) ?? Location.None;

	public static string Display(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

	private static string EscapeIdentifier(string name) =>
		SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None ? name : "@" + name;

	/// <summary>The assembly name as a C# namespace, followed by <c>.FluxorGenerated</c>.</summary>
	private static string GetNamespace(string? assemblyName)
	{
		IEnumerable<string> parts = (assemblyName ?? "Assembly")
			.Split('.')
			.Select(SanitizeIdentifier);
		return string.Join(".", parts) + ".FluxorGenerated";
	}

	private static string SanitizeIdentifier(string part)
	{
		var builder = new StringBuilder(part.Length + 1);
		foreach (char c in part)
			builder.Append(SyntaxFacts.IsIdentifierPartCharacter(c) ? c : '_');
		if (builder.Length == 0 || !SyntaxFacts.IsIdentifierStartCharacter(builder[0]))
			builder.Insert(0, '_');
		return EscapeIdentifier(builder.ToString());
	}
}
