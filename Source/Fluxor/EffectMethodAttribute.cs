using System;

namespace Fluxor
{
	///	<summary>
	///		Identifies a method as an Effect handler to be triggered in response to an action. This is an
	///		alternative to using <see cref="IEffect"/> or <see cref="Effect{TTriggerAction}"/> and
	///		will be discovered when using <see cref="DependencyInjection.Options.UseDependencyInjection(System.Reflection.Assembly[])"/>.
	///		<para>
	///			The format of the method must be ({ActionType} action, IDispatcher dispatcher) => Task
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
	///			public static Task HandleSomeOtherAction(MyAction2 action, IDispatcher dispatcher)
	///			{
	///				... do something else ...
	///			}
	///		}
	///	</example>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class EffectMethodAttribute : Attribute
	{
	}
}
