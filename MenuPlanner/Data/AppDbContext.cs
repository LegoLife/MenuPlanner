using Microsoft.EntityFrameworkCore;

namespace MenuPlanner.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Main> Mains => Set<Main>();
    public DbSet<Side> Sides => Set<Side>();
    public DbSet<PlanDay> PlanDays => Set<PlanDay>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlanDay>(day =>
        {
            day.HasIndex(d => d.Date).IsUnique();

            // Deleting a main leaves the day empty rather than deleting it.
            day.HasOne(d => d.Main)
                .WithMany()
                .HasForeignKey(d => d.MainId)
                .OnDelete(DeleteBehavior.SetNull);

            day.HasMany(d => d.Sides)
                .WithMany(s => s.PlanDays)
                .UsingEntity("PlanDaySide");
        });

        modelBuilder.Entity<Main>()
            .HasMany(m => m.Sides)
            .WithMany(s => s.Mains)
            .UsingEntity("MainSide", join => join.HasData(SeedData.MainSides));

        modelBuilder.Entity<Main>().HasData(SeedData.Mains);
        modelBuilder.Entity<Side>().HasData(SeedData.Sides);
    }
}
