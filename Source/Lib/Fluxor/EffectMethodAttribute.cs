using System;

namespace Fluxor
{
	///	<summary>
	///		Identifies a method as an Effect handler to be triggered in response to an action. This is an
	///		alternative to using <see cref="IEffect"/> or <see cref="Effect{TTriggerAction}"/> and
	///		will be discovered when using <see cref="DependencyInjection.Options.UseDependencyInjection(System.Reflection.Assembly[])"/>.
	///		<para>
	///			When no ActionType is specified <see cref="EffectMethodAttribute.EffectMethodAttribute()"/> then the method signature must be
	///				({ActionType} action, IDispatcher dispatcher) => Task
	///		</para>
	///		<para>
	///			When an ActionType is specified <see cref="EffectMethodAttribute.EffectMethodAttribute(Type)"/> then the method signature must be
	///				(IDispatcher dispatcher) => Task
	///		</para>
	///	</summary>
	///	<example>
	///		public static class AClassWithOneOrMoreEffects
	///		{
	///			[EffectMethod]
	///			public static Task HandleMyAction1(MyAction1 action, IDispatcher dispatcher)
	///			{
	///				... do something ...
	///			}
	///
	///			[EffectMethod]
	///			public static Task HandleMyAction2(MyAction2 action, IDispatcher dispatcher)
	///			{
	///				... do something else ...
	///			}
	///
	///			// or
	///
	///			[EffectMethod(typeof(MyAction1))]
	///			public static Task HandleMyAction1(IDispatcher dispatcher)
	///			{
	///				... do something ...
	///			}
	///		}
	///
	///			[EffectMethod(typeof(MyAction2))]
	///			public static Task HandleMyAction2(IDispatcher dispatcher)
	///			{
	///				... do something ...
	///			}
	///		}
	///	</example>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class EffectMethodAttribute : Attribute
	{
		public Type ActionType { get; private set; }

		///	<summary>
		///		Identifies a method with signature (object action, IDispatcher dispatcher) => Task as an effect method
		///	</summary>
		public EffectMethodAttribute() { }

		///	<summary>
		///		Identifies a method with signature (IDispatcher dispatcher) => Task as an effect method
		///	</summary>
		///	<param name="actionType>
		///		The type of the action that triggers this effect
		///	</param>
		public EffectMethodAttribute(Type actionType)
		{
			ActionType = actionType ?? throw new ArgumentNullException(nameof(actionType));
		}
	}
}
