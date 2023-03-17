using Microsoft.EntityFrameworkCore;

public class WeatherReportDbContext : DbContext
{
    public WeatherReportDbContext()
    {
    }
    public WeatherReportDbContext(DbContextOptions opts) : base(opts)
    {
    }
    public DbSet<WeatherReport> WeatherResports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        SnakeCaseIdentityTableNames(modelBuilder);
    }

    private void SnakeCaseIdentityTableNames(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherReport>(w => w.ToTable("weather_report"));
    }
}