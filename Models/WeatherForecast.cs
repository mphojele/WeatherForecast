namespace WeatherForecast.Models
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public string WeatherCondition { get; set; }

        public float TemperatureMin { get; set; }

        public float TemperatureMax { get; set; }
    }
}