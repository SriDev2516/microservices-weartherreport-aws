using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WeatherReportDbContext>
(
    opts =>
    {
        opts.EnableDetailedErrors();
        opts.EnableSensitiveDataLogging();
        opts.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
    }
);

var app = builder.Build();

app.Run();
