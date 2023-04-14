namespace WeatherForecast.WebAPIControllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using WeatherForecast.Models;

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastAPIController : ControllerBase
    {
        private static readonly HttpClient HttpClient = new ();

        private readonly ILogger<WeatherForecastAPIController> logger;
        private readonly IMemoryCache memoryCache;
        private readonly Uri forecastUrl = new ("https://api.open-meteo.com/v1/forecast");
        private readonly Uri geocodingUrl = new ("https://geocoding-api.open-meteo.com/v1/search");
        private readonly string cacheKey = "forecasts";

        public WeatherForecastAPIController(ILogger<WeatherForecastAPIController> logger, IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> GetAsync()
        {
            if (this.memoryCache.TryGetValue(this.cacheKey, out List<WeatherForecast> weeklyForecast))
            {
                return this.Ok(weeklyForecast);
            }

            var city = await this.GetCityAsync();
            var requestUrl = new StringBuilder(this.forecastUrl.ToString())
                .Append($"?latitude={city.Latitude.ToString(CultureInfo.InvariantCulture)}")
                .Append($"&longitude={city.Longitude.ToString(CultureInfo.InvariantCulture)}")
                .Append("&daily=weathercode,temperature_2m_max,temperature_2m_min&timezone=auto");
            var response = await HttpClient.GetAsync(requestUrl.ToString());
            var jsonString = await response.Content.ReadAsStringAsync();
            JsonNode? forecastNode = JsonNode.Parse(jsonString);

            var forecastResponse = JsonSerializer.Deserialize<WeatherForecastAPIResponse>(forecastNode["daily"]);
            weeklyForecast = new List<WeatherForecast>();
            for (int i = 0; i < forecastResponse.Dates.Count; i++)
            {
                weeklyForecast.Add(new WeatherForecast()
                {
                    Date = forecastResponse.Dates[i],
                    WeatherCondition = TranslateWeatherCode(forecastResponse.WeatherCodes[i]),
                    TemperatureMin = forecastResponse.TemperatureMin[i],
                    TemperatureMax = forecastResponse.TemperatureMax[i],
                });
            }

            this.memoryCache.Set(this.cacheKey, weeklyForecast, TimeSpan.FromHours(1));

            return this.Ok(weeklyForecast);
        }

        private async Task<City?> GetCityAsync()
        {
            var response = await HttpClient.GetAsync($"{this.geocodingUrl}?name=Johannesburg&count=1");
            var jsonString = await response.Content.ReadAsStringAsync();
            JsonNode? cityNode = JsonNode.Parse(jsonString);
            if (cityNode == null)
            {
                return null;
            }

            var cityInfo = JsonSerializer.Deserialize<List<City>>(cityNode["results"]);
            if (cityInfo == null)
            {
                return null;
            }

            return cityInfo[0];
        }

        private string TranslateWeatherCode(int code)
        {
            switch (code)
            {
                case 0:
                    return "Clear Sky";
                case 1:
                    return "Mainly Clear";
                case 2:
                    return "Partly Cloudy";
                case 3:
                    return "Overcast";
                case 45:
                    return "Fog";
                case 48:
                    return "Depositing Rime Fog";
                case 51:
                    return "Light Drizzle";
                case 53:
                    return "Moderate Drizzle";
                case 55:
                    return "Dense Drizzle";
                case 56:
                    return "Light Freezing Drizzle";
                case 57:
                    return "Dense Freezing Drizzle";
                case 61:
                    return "Slight Rain";
                case 63:
                    return "Moderate Rain";
                case 65:
                    return "Heavy Rain";
                case 66:
                    return "Light Freezing Rain";
                case 67:
                    return "Heavy Freezing Rain";
                case 71:
                    return "Slight Snow Fall";
                case 73:
                    return "Moderate Snow Fall";
                case 75:
                    return "Heavy Snow Fall";
                case 77:
                    return "Snow Grains";
                case 80:
                    return "Slight Rain Showers";
                case 81:
                    return "Moderate Rain Showers";
                case 82:
                    return "Violent Rain Showers";
                case 85:
                    return "Slight Snow Showers";
                case 86:
                    return "Heavy Snow Showers";
                case 95:
                    return "Thunderstorm";
                case 96:
                    return "Thunderstorm With Light Hail";
                case 99:
                    return "Thunderstorm With Heavy Hail";
                default:
                    return "Unknown";
            }
        }
    }
}