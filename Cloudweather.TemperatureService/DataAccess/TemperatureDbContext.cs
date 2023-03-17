using Microsoft.EntityFrameworkCore;

public class TemperatureDbContext : DbContext
{
    public TemperatureDbContext()
    {
    }
    public TemperatureDbContext(DbContextOptions opts) : base(opts)
    {
    }

    public DbSet<Temperature> Temperature { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        SnakeCaseIdentityTableNames(modelBuilder);
    }

    private void SnakeCaseIdentityTableNames(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Temperature>(t => t.ToTable("temperature"));
    }
}