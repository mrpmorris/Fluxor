using System;

namespace BasicConcepts.EffectsSample.Shared
{
	// This would typically be in another DLL containing API classes for a service
	public class WeatherForecast
	{
		public DateTime Date { get; set; }

		public int TemperatureC { get; set; }

		public string Summary { get; set; }

		public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
	}
}
