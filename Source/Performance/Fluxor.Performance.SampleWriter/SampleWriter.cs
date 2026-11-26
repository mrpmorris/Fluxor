using System.Globalization;
using System.Text;

namespace Fluxor.Performance.Samples;

/// <summary>
/// A file the <see cref="SampleWriter"/> writes.
/// </summary>
/// <param name="FileName">The file's name</param>
/// <param name="Content">The file's text</param>
/// <param name="IsBase">True for code of the separate base assembly (Complex profile), which does not opt in</param>
public sealed record SampleFile(string FileName, string Content, bool IsBase = false);

/// <summary>
/// Writes a Fluxor sample of a given size as C# source. Deterministic: no randomness, no dates, the same
/// options always give the same text. The sample is compiled twice, once with generated registration and once
/// scanned by reflection, to compare the two.
/// </summary>
public static class SampleWriter
{
	/// <summary>
	/// Writes the sample into <paramref name="directory"/>, and the code of its separate base assembly (Complex
	/// profile) into <paramref name="baseDirectory"/>, removing any other <c>*.cs</c> files there.
	/// </summary>
	public static IReadOnlyList<SampleFile> Write(SampleOptions options, string directory, string? baseDirectory = null)
	{
		IReadOnlyList<SampleFile> files = Create(options);
		SampleFile[] baseFiles = files.Where(x => x.IsBase).ToArray();
		if (baseFiles.Length > 0 && baseDirectory is null)
			throw new ArgumentException($"The {options.Profile} profile writes a base assembly: give a base directory", nameof(baseDirectory));

		WriteFiles(files.Where(x => !x.IsBase).ToArray(), directory);
		if (baseFiles.Length > 0)
			WriteFiles(baseFiles, baseDirectory!);
		return files;
	}

	/// <summary>Creates the sample's files without writing them.</summary>
	public static IReadOnlyList<SampleFile> Create(SampleOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);
		options.Validate();

		var layout = new Layout(options);
		var files = new List<SampleFile>
		{
			new("States.cs", WriteStates(layout)),
			new("Actions.cs", WriteActions(layout)),
			new("ReducerMethods.cs", WriteReducerMethods(layout)),
			new("EffectMethods.cs", WriteEffectMethods(layout))
		};
		if (layout.HasReducerClasses)
			files.Add(new("ReducerClasses.cs", WriteReducerClasses(layout)));
		if (layout.HasEffectClasses)
			files.Add(new("EffectClasses.cs", WriteEffectClasses(layout)));
		if (layout.StatesOfKind(StateKind.FeatureClass, StateKind.InheritedChain).Length > 0)
			files.Add(new("FeatureClasses.cs", WriteFeatureClasses(layout)));
		if (options.Profile == SampleProfile.Complex)
		{
			files.Add(new("InheritedHosts.cs", WriteInheritedHosts(layout)));
			files.Add(new("NestedTypes.cs", WriteNestedTypes(layout)));
			files.Add(new("PartialHostsA.cs", WritePartialHosts(layout, first: true)));
			files.Add(new("PartialHostsB.cs", WritePartialHosts(layout, first: false)));
			files.Add(new("BaseActions.cs", WriteBaseActions(layout)));
			files.Add(new("SharedActions.cs", WriteSharedActions(layout)));
			files.Add(new("Middleware.cs", WriteMiddleware(layout)));
			files.Add(new("ChainBases.cs", WriteChainBases(layout), IsBase: true));
		}
		if (options.PlainClasses > 0)
			files.Add(new("PlainClasses.cs", WritePlainClasses(layout)));
		return files;
	}

	private static void WriteFiles(IReadOnlyList<SampleFile> files, string directory)
	{
		Directory.CreateDirectory(directory);

		var fileNames = new HashSet<string>(files.Select(x => x.FileName), StringComparer.OrdinalIgnoreCase);
		foreach (string existing in Directory.GetFiles(directory, "*.cs"))
		{
			if (!fileNames.Contains(Path.GetFileName(existing)))
				File.Delete(existing);
		}

		var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
		foreach (SampleFile file in files)
		{
			string path = Path.Combine(directory, file.FileName);
			// Leave unchanged files alone so incremental builds stay incremental; Git may check them out with CRLF
			if (File.Exists(path) && NormalizeLineEndings(File.ReadAllText(path)) == NormalizeLineEndings(file.Content))
				continue;
			File.WriteAllText(path, file.Content, encoding);
		}
	}

	private static string NormalizeLineEndings(string text) => text.Replace("\r\n", "\n");

	private static string WriteStates(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: false);
		for (int state = 0; state < layout.Options.States; state++)
		{
			string name = layout.State(state);
			switch (layout.Kind(state))
			{
				// Declared with their own code
				case StateKind.InheritedChain:
				case StateKind.Nested:
					continue;

				case StateKind.PublicFactory:
					code.Line("[FeatureState(CreateInitialStateMethodName = nameof(Create))]");
					code.Line($"public record {name}(int Count, string Text)");
					code.Line("{");
					code.Line($"\tpublic static {name} Create() => new(0, \"\");");
					code.Line("}");
					break;

				case StateKind.PrivateFactory:
					code.Line("[FeatureState(CreateInitialStateMethodName = \"Create\")]");
					code.Line($"public record {name}(int Count, string Text)");
					code.Line("{");
					code.Line($"\tprivate static {name} Create() => new(0, \"\");");
					code.Line("}");
					break;

				case StateKind.RequiredMembers:
					code.Line("[FeatureState]");
					code.Line($"public record {name}");
					code.Line("{");
					code.Line("\tpublic required int Count { get; init; }");
					code.Line("\tpublic required string Text { get; init; }");
					code.Line("}");
					break;

				case StateKind.FeatureClass:
					code.Line($"public record {name}(int Count, string Text);");
					break;

				default:
					code.Line("[FeatureState]");
					code.Line($"public record {name}(int Count, string Text)");
					code.Line("{");
					string accessibility = layout.Kind(state) == StateKind.PrivateConstructor ? "private" : "public";
					code.Line($"\t{accessibility} {name}() : this(0, \"\") {{ }}");
					code.Line("}");
					break;
			}
			code.Line();
		}
		return code.ToString();
	}

	private static string WriteActions(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: false);
		for (int state = 0; state < layout.Options.States; state++)
		{
			// Declared with their own code
			if (layout.Kind(state) is StateKind.InheritedChain or StateKind.Nested or StateKind.BaseActions)
				continue;

			for (int action = 0; action < layout.ActionsOf(state); action++)
				code.Line($"public record {layout.Action(state, action)}(int Value);");
		}
		return code.ToString();
	}

	private static string WriteReducerMethods(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: false);
		SampleOptions options = layout.Options;
		for (int host = 0; host < options.ReducerHostClasses; host++)
		{
			int[] states = layout.StatesOfHost(host, options.ReducerHostClasses);
			if (states.Length == 0)
				continue;

			bool isStatic = layout.IsStaticReducerHost(host);
			string modifier = isStatic ? "static " : "";
			code.Line($"public {modifier}class {layout.ReducerHost(host)}");
			code.Line("{");
			bool first = true;
			foreach (int state in states)
			{
				for (int reducer = 0; reducer < options.ReducersOf(state); reducer++)
				{
					if (layout.IsReducerClass(state, reducer))
						continue;

					if (!first)
						code.Line();
					first = false;

					string stateName = layout.State(state);
					string actionName = layout.Action(state, reducer);
					string methodName = "Reduce" + actionName;
					if (layout.HasActionParameter(reducer))
					{
						code.Line("\t[ReducerMethod]");
						code.Line($"\tpublic {modifier}{stateName} {methodName}({stateName} state, {actionName} action) =>");
						code.Line("\t\tstate with { Count = state.Count + action.Value };");
					}
					else
					{
						code.Line($"\t[ReducerMethod(typeof({actionName}))]");
						code.Line($"\tpublic {modifier}{stateName} {methodName}({stateName} state) =>");
						code.Line("\t\tstate with { Count = state.Count + 1 };");
					}
				}
			}
			code.Line("}");
			code.Line();
		}
		return code.ToString();
	}

	private static string WriteEffectMethods(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: true);
		SampleOptions options = layout.Options;
		if (!layout.HasEffects)
			return code.ToString();

		for (int host = 0; host < options.EffectHostClasses; host++)
		{
			int[] states = layout.StatesOfHost(host, options.EffectHostClasses);
			if (states.Length == 0)
				continue;

			bool isStatic = layout.IsStaticEffectHost(host);
			string modifier = isStatic ? "static " : "";
			code.Line($"public {modifier}class {layout.EffectHost(host)}");
			code.Line("{");
			bool first = true;
			foreach (int state in states)
			{
				for (int effect = 0; effect < options.EffectsOf(state); effect++)
				{
					if (layout.IsEffectClass(state, effect))
						continue;

					if (!first)
						code.Line();
					first = false;

					string actionName = layout.Action(state, effect);
					string methodName = $"Handle{actionName}Effect{effect}";
					if (layout.HasActionParameter(effect))
					{
						code.Line("\t[EffectMethod]");
						code.Line($"\tpublic {modifier}Task {methodName}({actionName} action, IDispatcher dispatcher) =>");
					}
					else
					{
						code.Line($"\t[EffectMethod(typeof({actionName}))]");
						code.Line($"\tpublic {modifier}Task {methodName}(IDispatcher dispatcher) =>");
					}
					code.Line("\t\tTask.CompletedTask;");
				}
			}
			code.Line("}");
			code.Line();
		}
		return code.ToString();
	}

	private static string WriteReducerClasses(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: false);
		for (int state = 0; state < layout.Options.States; state++)
		{
			for (int reducer = 0; reducer < layout.Options.ReducersOf(state); reducer++)
			{
				if (!layout.IsReducerClass(state, reducer))
					continue;

				string stateName = layout.State(state);
				string actionName = layout.Action(state, reducer);
				code.Line($"public class {actionName}Reducer : IReducer<{stateName}>");
				code.Line("{");
				code.Line($"\tpublic {stateName} Reduce({stateName} state, object action) =>");
				code.Line($"\t\tstate with {{ Count = state.Count + (({actionName})action).Value }};");
				code.Line();
				code.Line($"\tpublic bool ShouldReduceStateForAction(object action) => action is {actionName};");
				code.Line("}");
				code.Line();
			}
		}
		return code.ToString();
	}

	private static string WriteEffectClasses(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: true);
		for (int state = 0; state < layout.Options.States; state++)
		{
			for (int effect = 0; effect < layout.Options.EffectsOf(state); effect++)
			{
				if (!layout.IsEffectClass(state, effect))
					continue;

				string actionName = layout.Action(state, effect);
				code.Line($"public class {actionName}Effect{effect} : IEffect");
				code.Line("{");
				code.Line("\tpublic Task HandleAsync(object action, IDispatcher dispatcher) => Task.CompletedTask;");
				code.Line();
				code.Line($"\tpublic bool ShouldReactToAction(object action) => action is {actionName};");
				code.Line("}");
				code.Line();
			}
		}
		return code.ToString();
	}

	private static string WriteFeatureClasses(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: false);
		foreach (int state in layout.StatesOfKind(StateKind.FeatureClass, StateKind.InheritedChain))
		{
			string name = layout.State(state);
			// The inheritance chains' states are declared in the base assembly, which the app does not scan
			string type = layout.Kind(state) == StateKind.InheritedChain ? "Base." + name : name;
			code.Line($"public class {name}Feature : Feature<{type}>");
			code.Line("{");
			code.Line($"\tpublic override string GetName() => \"{name}\";");
			code.Line();
			code.Line($"\tprotected override {type} GetInitialState() => new(0, \"\");");
			code.Line("}");
			code.Line();
		}
		return code.ToString();
	}

	/// <summary>
	/// The first level of each inheritance chain, with its state and actions, in the separate base assembly:
	/// a static and a virtual reducer, an instance and a virtual effect.
	/// </summary>
	private static string WriteChainBases(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: true, namespaceSuffix: ".Base");
		foreach (int state in layout.StatesOfKind(StateKind.InheritedChain))
		{
			string s = layout.State(state);
			code.Line($"public record {s}(int Count, string Text)");
			code.Line("{");
			code.Line($"\tpublic {s}() : this(0, \"\") {{ }}");
			code.Line("}");
			code.Line();
			for (int action = 0; action < 10; action++)
				code.Line($"public record {layout.Action(state, action)}(int Value);");
			code.Line();
			code.Line($"public abstract class {s}Level1");
			code.Line("{");
			var members = new MemberWriter(code, "\t", s, x => layout.Action(state, x));
			members.Reducer(0, isStatic: true, withAction: true);
			members.Reducer(1, isStatic: false, withAction: false, modifier: "virtual ");
			members.Effect(0, isStatic: false, withAction: true);
			members.Effect(1, isStatic: false, withAction: false, modifier: "virtual ");
			code.Line("}");
			code.Line();
		}
		return code.ToString();
	}

	/// <summary>
	/// The second and third levels of each inheritance chain: both are registered, each with every method it sees.
	/// Level 2 sees 3 reducers and 3 effects; level 3 overrides level 1's virtual methods, hides level 2's reducer with
	/// <c>new</c> (both are registered) and adds its own, to see 7 of each.
	/// </summary>
	private static string WriteInheritedHosts(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: true);
		foreach (int state in layout.StatesOfKind(StateKind.InheritedChain))
		{
			string s = layout.State(state);
			var members = new MemberWriter(code, "\t", "Base." + s, x => "Base." + layout.Action(state, x));

			code.Line($"public class {s}Level2 : Base.{s}Level1");
			code.Line("{");
			members.Reducer(2, isStatic: false, withAction: true);
			members.Effect(2, isStatic: true, withAction: true);
			code.Line("}");
			code.Line();

			code.Line($"public class {s}Level3 : {s}Level2");
			code.Line("{");
			members.StartClass();
			members.Reducer(1, isStatic: false, withAction: false, modifier: "override ");
			members.Reducer(2, isStatic: false, withAction: true, modifier: "new ");
			members.Reducer(3, isStatic: true, withAction: true);
			members.Reducer(4, isStatic: false, withAction: false);
			members.Reducer(5, isStatic: false, withAction: true);
			members.Effect(1, isStatic: false, withAction: false, modifier: "override ");
			members.Effect(3, isStatic: true, withAction: true);
			members.Effect(4, isStatic: false, withAction: false);
			members.Effect(5, isStatic: false, withAction: true);
			members.Effect(6, isStatic: true, withAction: false);
			code.Line("}");
			code.Line();
		}
		return code.ToString();
	}

	/// <summary>Each state, its actions and its hosts nested in a class, with a second level of nesting and an Effect&lt;T&gt; class.</summary>
	private static string WriteNestedTypes(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: true);
		foreach (int state in layout.StatesOfKind(StateKind.Nested))
		{
			string s = layout.State(state);
			code.Line($"public class {s}Nest");
			code.Line("{");
			code.Line("\t[FeatureState]");
			code.Line($"\tpublic record {s}(int Count, string Text)");
			code.Line("\t{");
			code.Line($"\t\tpublic {s}() : this(0, \"\") {{ }}");
			code.Line("\t}");
			code.Line();
			for (int action = 0; action < 10; action++)
				code.Line($"\tpublic record {layout.Action(state, action)}(int Value);");
			code.Line();

			var outer = new MemberWriter(code, "\t\t", s, x => layout.Action(state, x));
			var inner = new MemberWriter(code, "\t\t\t", s, x => layout.Action(state, x));
			code.Line("\tpublic class Reducers");
			code.Line("\t{");
			for (int reducer = 0; reducer < 5; reducer++)
				outer.Reducer(reducer, isStatic: reducer % 2 == 0, withAction: reducer % 2 == 0);
			code.Line();
			code.Line("\t\tpublic class Deeper");
			code.Line("\t\t{");
			for (int reducer = 5; reducer < 10; reducer++)
				inner.Reducer(reducer, isStatic: reducer % 2 == 0, withAction: reducer % 2 == 1);
			code.Line("\t\t}");
			code.Line("\t}");
			code.Line();
			code.Line("\tpublic class Effects");
			code.Line("\t{");
			outer.StartClass();
			inner.StartClass();
			for (int effect = 0; effect < 5; effect++)
				outer.Effect(effect, isStatic: effect % 2 == 1, withAction: effect % 2 == 0);
			code.Line();
			code.Line("\t\tpublic class Deeper");
			code.Line("\t\t{");
			for (int effect = 5; effect < 9; effect++)
				inner.Effect(effect, isStatic: effect % 2 == 0, withAction: effect % 2 == 1);
			code.Line("\t\t}");
			code.Line("\t}");
			code.Line();
			string lastAction = layout.Action(state, 9);
			code.Line($"\tpublic class {lastAction}Effect : Effect<{lastAction}>");
			code.Line("\t{");
			code.Line($"\t\tpublic override Task HandleAsync({lastAction} action, IDispatcher dispatcher) => Task.CompletedTask;");
			code.Line("\t}");
			code.Line("}");
			code.Line();
		}
		return code.ToString();
	}

	/// <summary>One host per state, declared partial in two files: half of its reducers and effects in each.</summary>
	private static string WritePartialHosts(Layout layout, bool first)
	{
		var code = new CodeBuilder(layout, usesTasks: true);
		int from = first ? 0 : 5;
		foreach (int state in layout.StatesOfKind(StateKind.PartialHost))
		{
			string s = layout.State(state);
			var members = new MemberWriter(code, "\t", s, x => layout.Action(state, x));
			code.Line($"public partial class {s}Host");
			code.Line("{");
			for (int index = from; index < from + 5; index++)
				members.Reducer(index, isStatic: index % 2 == 0, withAction: index % 3 != 2);
			for (int index = from; index < from + 5; index++)
				members.Effect(index, isStatic: index % 2 == 1, withAction: index % 3 != 2);
			code.Line("}");
			code.Line();
		}
		return code.ToString();
	}

	/// <summary>
	/// Actions deriving from a base record and implementing an interface; a reducer on the base action, effects on the
	/// base action and on the interface (with and without an action parameter), the rest on concrete actions.
	/// </summary>
	private static string WriteBaseActions(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: true);
		foreach (int state in layout.StatesOfKind(StateKind.BaseActions))
		{
			string s = layout.State(state);
			string baseAction = s + "BaseAction";
			string marker = "I" + s + "Marker";
			code.Line($"public interface {marker} {{ }}");
			code.Line();
			code.Line($"public abstract record {baseAction}(int Value);");
			code.Line();
			for (int action = 0; action < 10; action++)
				code.Line($"public record {layout.Action(state, action)}(int Value) : {baseAction}(Value), {marker};");
			code.Line();

			var members = new MemberWriter(code, "\t", s, x => layout.Action(state, x));
			code.Line($"public class {s}Host");
			code.Line("{");
			for (int reducer = 0; reducer < 9; reducer++)
				members.Reducer(reducer, isStatic: reducer % 2 == 0, withAction: reducer % 3 != 2);
			members.ReducerOn(baseAction, isStatic: true);
			members.EffectOn(baseAction, isStatic: false, withAction: true);
			members.EffectOn(marker, isStatic: true, withAction: true);
			members.EffectOn(baseAction, isStatic: true, withAction: false);
			members.EffectOn(marker, isStatic: false, withAction: false);
			for (int effect = 0; effect < 6; effect++)
				members.Effect(effect, isStatic: effect % 2 == 1, withAction: effect % 3 != 2);
			code.Line("}");
			code.Line();
		}
		return code.ToString();
	}

	/// <summary>Two reducers and two effects for each of five actions, in an order that changes the result.</summary>
	private static string WriteSharedActions(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: true);
		foreach (int state in layout.StatesOfKind(StateKind.SharedActions))
		{
			string s = layout.State(state);
			code.Line($"public class {s}Host");
			code.Line("{");
			for (int action = 0; action < 5; action++)
			{
				string a = layout.Action(state, action);
				if (action > 0)
					code.Line();
				code.Line("\t[ReducerMethod]");
				code.Line($"\tpublic static {s} Reduce{a}First({s} state, {a} action) =>");
				code.Line("\t\tstate with { Count = state.Count + action.Value };");
				code.Line();
				code.Line($"\t[ReducerMethod(typeof({a}))]");
				code.Line($"\tpublic {s} Reduce{a}Second({s} state) =>");
				code.Line("\t\tstate with { Count = state.Count * 2 };");
				code.Line();
				code.Line("\t[EffectMethod]");
				code.Line($"\tpublic Task Handle{a}First({a} action, IDispatcher dispatcher) =>");
				code.Line("\t\tTask.CompletedTask;");
				code.Line();
				code.Line($"\t[EffectMethod(typeof({a}))]");
				code.Line($"\tpublic static Task Handle{a}Second(IDispatcher dispatcher) =>");
				code.Line("\t\tTask.CompletedTask;");
			}
			code.Line("}");
			code.Line();
		}
		return code.ToString();
	}

	/// <summary>A middleware namespace: the scan leaves out the Fluxor code next to a middleware, unless the app adds it.</summary>
	private static string WriteMiddleware(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: true, namespaceSuffix: ".WithMiddleware");
		code.Line("public class SampleMiddleware : Middleware");
		code.Line("{");
		code.Line("}");
		code.Line();
		code.Line("// Left out by the scan, as the namespace has a middleware the app does not add");
		code.Line("[FeatureState]");
		code.Line("public record ExcludedState(int Count, string Text)");
		code.Line("{");
		code.Line("\tpublic ExcludedState() : this(0, \"\") { }");
		code.Line("}");
		code.Line();
		code.Line("public record ExcludedAction(int Value);");
		code.Line();
		code.Line("public class ExcludedHost");
		code.Line("{");
		var members = new MemberWriter(code, "\t", "ExcludedState", _ => "ExcludedAction");
		members.Reducer(0, isStatic: true, withAction: true);
		members.Effect(0, isStatic: false, withAction: true);
		code.Line("}");
		return code.ToString();
	}

	private static string WritePlainClasses(Layout layout)
	{
		var code = new CodeBuilder(layout, usesTasks: false, namespaceSuffix: ".Plain");
		SampleOptions options = layout.Options;
		for (int plain = 0; plain < options.PlainClasses; plain++)
		{
			code.Line($"public class {layout.PlainClass(plain)}");
			code.Line("{");
			for (int method = 0; method < options.MethodsPerPlainClass; method++)
			{
				if (method > 0)
					code.Line();
				code.Line($"\tpublic int Method{method}(int value) => value + {method};");
			}
			code.Line("}");
			code.Line();
		}
		return code.ToString();
	}

	/// <summary>How a state, its actions and its reducers and effects are declared.</summary>
	private enum StateKind
	{
		Plain,
		PrivateConstructor,
		PublicFactory,
		PrivateFactory,
		RequiredMembers,
		FeatureClass,
		InheritedChain,
		Nested,
		PartialHost,
		BaseActions,
		SharedActions
	}

	/// <summary>
	/// Writes the reducer and effect methods of one state, blank line separated, at an indent. Method names are
	/// Reduce/Handle + action name, so a name says which action a method is for.
	/// </summary>
	private sealed class MemberWriter
	{
		private readonly CodeBuilder Code;
		private readonly string Indent;
		private readonly string StateType;
		private readonly Func<int, string> ActionType;
		private bool IsFirst = true;

		public MemberWriter(CodeBuilder code, string indent, string stateType, Func<int, string> actionType)
		{
			Code = code;
			Indent = indent;
			StateType = stateType;
			ActionType = actionType;
		}

		public void Reducer(int action, bool isStatic, bool withAction, string modifier = "") =>
			WriteReducer(ActionType(action), isStatic, withAction, modifier);

		public void ReducerOn(string actionType, bool isStatic) =>
			WriteReducer(actionType, isStatic, withAction: true, modifier: "");

		public void Effect(int action, bool isStatic, bool withAction, string modifier = "") =>
			WriteEffect(ActionType(action), isStatic, withAction, modifier);

		public void EffectOn(string actionType, bool isStatic, bool withAction) =>
			WriteEffect(actionType, isStatic, withAction, modifier: "");

		private void WriteReducer(string actionType, bool isStatic, bool withAction, string modifier)
		{
			Separate();
			string name = "Reduce" + Simple(actionType) + (withAction ? "" : "WithoutAction");
			string staticModifier = isStatic ? "static " : "";
			if (withAction)
			{
				Code.Line(Indent + "[ReducerMethod]");
				Code.Line($"{Indent}public {staticModifier}{modifier}{StateType} {name}({StateType} state, {actionType} action) =>");
				Code.Line(Indent + "\tstate with { Count = state.Count + action.Value };");
			}
			else
			{
				Code.Line($"{Indent}[ReducerMethod(typeof({actionType}))]");
				Code.Line($"{Indent}public {staticModifier}{modifier}{StateType} {name}({StateType} state) =>");
				Code.Line(Indent + "\tstate with { Count = state.Count + 1 };");
			}
		}

		private void WriteEffect(string actionType, bool isStatic, bool withAction, string modifier)
		{
			Separate();
			string name = "Handle" + Simple(actionType) + (withAction ? "" : "WithoutAction");
			string staticModifier = isStatic ? "static " : "";
			if (withAction)
			{
				Code.Line(Indent + "[EffectMethod]");
				Code.Line($"{Indent}public {staticModifier}{modifier}Task {name}({actionType} action, IDispatcher dispatcher) =>");
			}
			else
			{
				Code.Line($"{Indent}[EffectMethod(typeof({actionType}))]");
				Code.Line($"{Indent}public {staticModifier}{modifier}Task {name}(IDispatcher dispatcher) =>");
			}
			Code.Line(Indent + "\tTask.CompletedTask;");
		}

		/// <summary>The next member is the first of a class: no blank line before it.</summary>
		public void StartClass() => IsFirst = true;

		private void Separate()
		{
			if (!IsFirst)
				Code.Line();
			IsFirst = false;
		}

		private static string Simple(string typeName) => typeName[(typeName.LastIndexOf('.') + 1)..];
	}

	/// <summary>Names and the deterministic choice of which form each member takes.</summary>
	private sealed class Layout
	{
		public readonly SampleOptions Options;
		private readonly bool UsesStandardForms;
		private readonly bool UsesClasses;
		private readonly string StateFormat;
		private readonly string PlainFormat;
		private readonly int[] GeneralStates;

		public Layout(SampleOptions options)
		{
			Options = options;
			UsesStandardForms = options.Profile is SampleProfile.Standard or SampleProfile.Complex;
			UsesClasses = options.Profile != SampleProfile.Simple;
			StateFormat = "D" + Math.Max(3, Digits(options.States - 1));
			PlainFormat = "D" + Math.Max(4, Digits(options.PlainClasses - 1));
			GeneralStates = Enumerable.Range(0, options.States).Where(IsGeneral).ToArray();
		}

		public bool HasReducerClasses => UsesClasses && Options.ReducerClasses > 0;
		public bool HasEffects => Enumerable.Range(0, Options.States).Any(x => Options.EffectsOf(x) > 0);
		public bool HasEffectClasses => UsesClasses && Options.EffectClasses > 0 && HasEffects;

		public string State(int state) => "State" + Number(state, StateFormat);
		public string Action(int state, int action) => State(state) + "Action" + Number(action, "D");
		public string ReducerHost(int host) => "ReducerHost" + Number(host, StateFormat);
		public string EffectHost(int host) => "EffectHost" + Number(host, StateFormat);
		public string PlainClass(int plain) => "Plain" + Number(plain, PlainFormat);

		public int ActionsOf(int state) => Math.Max(Options.ReducersOf(state), Options.EffectsOf(state));

		public StateKind Kind(int state) => Options.Profile switch
		{
			SampleProfile.Standard when IsSpread(state, Options.PrivateConstructorStates, offset: 0) => StateKind.PrivateConstructor,
			SampleProfile.SimpleWithClasses when IsSpread(state, Options.FeatureClassStates, offset: HalfStep(Options.FeatureClassStates)) =>
				StateKind.FeatureClass,
			SampleProfile.Complex => (state % 10) switch
			{
				0 => StateKind.PrivateConstructor,
				1 => StateKind.InheritedChain,
				2 => state / 10 % 2 == 0 ? StateKind.PublicFactory : StateKind.PrivateFactory,
				3 => StateKind.RequiredMembers,
				4 => StateKind.FeatureClass,
				5 => StateKind.Nested,
				6 => StateKind.PartialHost,
				7 => StateKind.BaseActions,
				8 => StateKind.SharedActions,
				_ => StateKind.Plain
			},
			_ => StateKind.Plain
		};

		/// <summary>States whose reducers and effects are written to the shared host classes.</summary>
		private bool IsGeneral(int state) =>
			Kind(state) is StateKind.Plain or StateKind.PrivateConstructor or StateKind.PublicFactory or StateKind.PrivateFactory
				or StateKind.RequiredMembers or StateKind.FeatureClass;

		public int[] StatesOfKind(params StateKind[] kinds) =>
			Enumerable.Range(0, Options.States).Where(x => kinds.Contains(Kind(x))).ToArray();

		/// <summary>The last reducer of an evenly spread set of states is a reducer class.</summary>
		public bool IsReducerClass(int state, int reducer) =>
			HasReducerClasses
			&& reducer == Options.ReducersOf(state) - 1
			&& IsGeneral(state)
			&& IsSpread(state, Options.ReducerClasses, offset: 0);

		/// <summary>The last effect of an evenly spread set of states (half a step after the reducer classes) is an effect class.</summary>
		public bool IsEffectClass(int state, int effect) =>
			HasEffectClasses
			&& effect == Options.EffectsOf(state) - 1
			&& IsGeneral(state)
			&& IsSpread(state, Options.EffectClasses, offset: HalfStep(Options.EffectClasses));

		/// <summary>Every third method has no action parameter and names its action in the attribute.</summary>
		public bool HasActionParameter(int index) => !UsesStandardForms || index % 3 != 2;

		public bool IsStaticReducerHost(int host) => !UsesStandardForms || host % 2 == 0;

		public bool IsStaticEffectHost(int host) => UsesStandardForms && host % 4 == 0;

		/// <summary>The states whose reducers and effects go to shared host classes are dealt round-robin to them.</summary>
		public int[] StatesOfHost(int host, int hostCount) =>
			GeneralStates
				.Where((_, index) => index % hostCount == host)
				.ToArray();

		private int HalfStep(int count) => count == 0 ? 0 : Options.States / (2 * count);

		/// <summary>
		/// True for exactly <paramref name="count"/> of the states, spread evenly: k * count mod States takes each
		/// multiple of gcd(count, States) gcd times, so exactly count of them are below count.
		/// </summary>
		private bool IsSpread(int state, int count, int offset) =>
			count > 0 && (long)((state + offset) % Options.States) * count % Options.States < count;

		private static string Number(int value, string format) => value.ToString(format, CultureInfo.InvariantCulture);

		private static int Digits(int value) => Math.Max(1, value).ToString(CultureInfo.InvariantCulture).Length;
	}

	private sealed class CodeBuilder
	{
		private readonly StringBuilder Builder = new();

		public CodeBuilder(Layout layout, bool usesTasks, string namespaceSuffix = "")
		{
			Line("// Written by Fluxor.Performance.SampleWriter. Do not edit: change and run the writer instead.");
			Line("// " + Describe(layout.Options));
			Line();
			if (usesTasks)
			{
				Line("using System.Threading.Tasks;");
				Line();
			}
			Line($"namespace {layout.Options.Namespace}{namespaceSuffix};");
			Line();
		}

		public void Line(string text = "") => Builder.Append(text).Append('\n');

		public override string ToString() => Builder.ToString().TrimEnd('\n') + "\n";

		private static string Describe(SampleOptions options) =>
			options.TotalReducers is null && options.TotalEffects is null
				? string.Create(
					CultureInfo.InvariantCulture,
					$"profile {options.Profile}, {options.States} states, {options.ReducersPerState} reducers and"
					+ $" {options.EffectsPerState} effects per state, {options.PlainClasses} plain classes")
				: string.Create(
					CultureInfo.InvariantCulture,
					$"profile {options.Profile}, {options.States} states,"
					+ $" {Enumerable.Range(0, options.States).Sum(options.ReducersOf)} reducers and"
					+ $" {Enumerable.Range(0, options.States).Sum(options.EffectsOf)} effects, {options.PlainClasses} plain classes");
	}
}
