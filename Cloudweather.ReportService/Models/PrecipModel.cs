namespace Cloudweather.ReportService.Models
{
    public class PrecipModel
    {
        public decimal AmountInches { get; set; }
        public string WeatherType { get; set; } = string.Empty;
    }
}