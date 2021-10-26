﻿using Fluxor;

namespace BasicConcepts.ActionSubscriber.Store.EditCustomerUseCase
{
	[FeatureState]
	public class EditCustomerState
	{
		public bool IsLoading { get; private set; }

		private EditCustomerState() { }
		public EditCustomerState(bool isLoading)
		{
			IsLoading = isLoading;
		}
	}
}
