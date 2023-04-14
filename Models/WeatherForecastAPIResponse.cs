namespace WeatherForecast.Models
{
    using System.Text.Json.Serialization;

    public class WeatherForecastAPIResponse
    {
        [JsonPropertyName("time")]
        public List<DateTime> Dates { get; set; }

        [JsonPropertyName("weathercode")]
        public List<int> WeatherCodes { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public List<float> TemperatureMin { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public List<float> TemperatureMax { get; set; }
    }
}