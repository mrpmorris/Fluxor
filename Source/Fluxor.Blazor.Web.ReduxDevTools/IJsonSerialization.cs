using System;

namespace Fluxor.Blazor.Web.ReduxDevTools
{
	public interface IJsonSerialization
	{
		object Deserialize(string json, Type type);
		string Serialize(object source, Type type);
	}

	public static class IJsonSerializationExtensions
	{
		public static T Deserialize<T>(this IJsonSerialization instance, string json) =>
			(T)instance.Deserialize(json, typeof(T));
	}
}
