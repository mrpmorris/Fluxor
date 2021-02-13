using System;
using System.Text.Json;

namespace Fluxor.Blazor.Web.ReduxDevTools.Serialization
{
	public class SystemTextSerialization : IJsonSerialization
	{
		private readonly JsonSerializerOptions Options;

		public SystemTextSerialization(JsonSerializerOptions options = null)
		{
			Options = options;
		}

		public object Deserialize(string json, Type type) =>
			JsonSerializer.Deserialize(json, type, Options);

		public string Serialize(object source, Type type) =>
			JsonSerializer.Serialize(source, type, Options);
	}
}
