using System;

namespace Fluxor;

public interface IStateChangedNotifier
{
	/// <summary>
	/// Event that is executed whenever the observed value of the state changes
	/// </summary>
	event EventHandler StateChanged;
}