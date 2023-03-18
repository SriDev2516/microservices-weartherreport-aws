using System.Text.Json;
using Cloudweather.ReportService.Config;
using Cloudweather.ReportService.Models;
using Microsoft.Extensions.Options;

public interface IWeatherReportAggregator
{
    public Task<WeatherReport> BuildWeatherReport(string zip, int days);
}

public class WeatherReportAggregator : IWeatherReportAggregator
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger _logger;
    private readonly WeatherDataConfig _weatherDataConfig;
    private readonly WeatherReportDbContext _db;

    public WeatherReportAggregator(IHttpClientFactory http,
    ILogger<WeatherReportAggregator> logger,
    IOptions<WeatherDataConfig> weatherDataConfig,
    WeatherReportDbContext db
    )
    {
        _http = http;
        _logger = logger;
        _weatherDataConfig = weatherDataConfig.Value;
        _db = db;
    }
    public async Task<WeatherReport> BuildWeatherReport(string zip, int days)
    {
        var httpClient = _http.CreateClient();

        var precipData = await FetchPrecipData(httpClient, zip, days);
        var totalSnow = GetTotalSnow(precipData);
        var totalRain = GetTotalRain(precipData);

        _logger.LogInformation(
            $"zip: {zip} over last {days} days" +
            $"total snow: {totalSnow}, total rain: {totalRain}"
        );

        var tempData = await FetchTempData(httpClient, zip, days);
        var averageHighTemp = tempData.Average(t => t.TempHighF);
        var averageLowTemp = tempData.Average(t => t.TempLowF);

        var weatherReport = new WeatherReport()
        {
            AverageHigh = averageHighTemp,
            AverageLow = averageLowTemp,
            RainfallTotalInches = totalRain,
            SnowTotalInches = totalSnow,
            ZipCode = zip,
            CreatedOn = DateTime.UtcNow
        };

        _db.Add(weatherReport);
        await _db.SaveChangesAsync();

        return weatherReport;

    }

    private decimal GetTotalRain(List<PrecipModel> precipData)
    {
        var totalRain = precipData
            .Where(p => p.WeatherType == "snow")
            .Sum(p => p.AmountInches);
        return Math.Round(totalRain, 1);
    }

    private decimal GetTotalSnow(List<PrecipModel> precipData)
    {
        var totalSnow = precipData
            .Where(p => p.WeatherType == "rain")
            .Sum(p => p.AmountInches);
        return Math.Round(totalSnow, 1);
    }

    private async Task<List<TemperatureModel>> FetchTempData(HttpClient httpClient, string zip, int days)
    {
        var endpoint = BuildTempServiceEndPoints(zip, days);
        var temperatureRecords = await httpClient.GetAsync(endpoint);
        var jsonSerializaterOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var temperatureData = await temperatureRecords
                .Content
                .ReadFromJsonAsync<List<TemperatureModel>>(jsonSerializaterOptions);
        return temperatureData ?? new List<TemperatureModel>();
    }

    private string BuildTempServiceEndPoints(string zip, int days)
    {
        var tempServiceProtocol = _weatherDataConfig.TempDataProtocol;
        var tempServiceHost = _weatherDataConfig.TempDataHost;
        var tempServicePort = _weatherDataConfig.TempDataPort;

        return $"{tempServiceProtocol}://{tempServiceHost}:{tempServicePort}/observation/{zip}?days={days}";
    }

    private async Task<List<PrecipModel>> FetchPrecipData(HttpClient httpClient, string zip, int days)
    {
        var endpoint = BuildPrecipServiceEndPoints(zip, days);
        var precipRecords = await httpClient.GetAsync(endpoint);
        var jsonSerializaterOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var precipData = await precipRecords
            .Content
            .ReadFromJsonAsync<List<PrecipModel>>(jsonSerializaterOptions);

        return precipData ?? new List<PrecipModel>();

    }

    private string BuildPrecipServiceEndPoints(string zip, int days)
    {
        var precipServiceProtocol = _weatherDataConfig.PrecipDataProtocol;
        var precipServiceHost = _weatherDataConfig.PrecipDataHost;
        var precipServicePort = _weatherDataConfig.PrecipDataPort;

        return $"{precipServiceProtocol}://{precipServiceHost}:{precipServicePort}/observation/{zip}?days={days}";
    }
}