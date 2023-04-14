namespace Payroll.Validators
{
    using FluentValidation;

    using WeatherForecast.Models;

    public class WeatherForecastValidator : AbstractValidator<WeatherForecast>
    {
        public WeatherForecastValidator()
        {
        }
    }
}
