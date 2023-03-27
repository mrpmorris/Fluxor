using Fluxor.PreScanningStoreBuilder.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Fluxor.PreScanningStoreBuilder.CodeModels
{
	internal readonly struct FeatureStateInfo : IEquatable<FeatureStateInfo>
	{
		public readonly ClassDeclarationSyntax ClassDeclarationSyntax;
		public readonly string Name;
		public readonly string CreateInitialStateMethodName;
		public readonly byte MaximumStateChangedNotificationsPerSecond;

		public FeatureStateInfo(ClassDeclarationSyntax classDeclarationSyntax, string name, string createInitialStateMethodName, byte maximumStateChangedNotificationsPerSecond)
		{
			ClassDeclarationSyntax = classDeclarationSyntax;
			Name = name;
			CreateInitialStateMethodName = createInitialStateMethodName;
			MaximumStateChangedNotificationsPerSecond = maximumStateChangedNotificationsPerSecond;
		}

		public override bool Equals(object obj) =>
			obj switch {
				FeatureStateInfo other => Equals(other),
				_ => false
			};

		public bool Equals(FeatureStateInfo other) =>
				other.ClassDeclarationSyntax == ClassDeclarationSyntax
				&& other.Name == Name
				&& other.CreateInitialStateMethodName == CreateInitialStateMethodName
				&& other.MaximumStateChangedNotificationsPerSecond == MaximumStateChangedNotificationsPerSecond;

		public static bool operator ==(FeatureStateInfo left, FeatureStateInfo right) =>
				left.Equals(right);

		public static bool operator !=(FeatureStateInfo left, FeatureStateInfo right) =>
				!left.Equals(right);

		public override int GetHashCode() =>
			HashCode.Combine(ClassDeclarationSyntax, Name, CreateInitialStateMethodName, MaximumStateChangedNotificationsPerSecond);
	}
}
