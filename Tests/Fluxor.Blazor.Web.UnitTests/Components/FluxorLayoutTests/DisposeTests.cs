﻿using Fluxor.Blazor.Web.UnitTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.Blazor.Web.UnitTests.Components.FluxorLayoutTests
{
	public class DisposeTests
	{
		private readonly FluxorLayoutWithStateProperties StateSubject;
		private readonly MockState<int> MockState1;
		private readonly MockState<int> MockState2;

		[Fact]
		public void UnsubscribesFromStateProperties()
		{
			StateSubject.ExecuteOnInitialized();
			StateSubject.Dispose();

			Assert.Equal(1, MockState1.UnsubscribeCount);
			Assert.Equal(1, MockState2.UnsubscribeCount);
		}

		[Fact]
		public void WhenBaseOnInitializedWasNotCalled_ThenThrowsNullReferenceException()
		{
			string errorMessage = null;
			var layout = new FluxorLayoutThatOptionallyCallsBaseOnInitialized();
			try
			{
				layout.Test_OnInitialized();
				layout.Dispose();
			}
			catch (NullReferenceException e)
			{
				errorMessage = e.Message;
			}
			Assert.Equal("Have you forgotten to call base.OnInitialized() in your component?", errorMessage);
		}

		[Fact]
		public void WhenBaseOnInitializedWasCalled_ThenDoesNotThrowAnException()
		{
			var layout = new FluxorLayoutThatOptionallyCallsBaseOnInitialized
			{
				CallBaseOnInitialized = true
			};
			layout.Test_OnInitialized();
			layout.Dispose();
		}
		public DisposeTests()
		{
			MockState1 = new MockState<int>();
			MockState2 = new MockState<int>();
			StateSubject = new FluxorLayoutWithStateProperties
			{
				State1 = MockState1,
				State2 = MockState2
			};
		}
	}
}
