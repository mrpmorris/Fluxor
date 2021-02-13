using Newtonsoft.Json;
using System;

namespace Fluxor.Blazor.Web.ReduxDevTools.Serialization
{
	public class NewtonsoftJsonAdapter : IJsonSerialization
	{
		private readonly JsonSerializerSettings Settings;

		public NewtonsoftJsonAdapter(JsonSerializerSettings settings = null)
		{
			Settings = settings;
		}

		public object Deserialize(string json, Type type) =>
			JsonConvert.DeserializeObject(json, type, Settings);

		public string Serialize(object source, Type type) =>
			JsonConvert.SerializeObject(source, type, Settings);
	}
}
