using MenuPlanner.Data;
using MenuPlanner.Planning;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MenuPlanner.Tests;

/// <summary>Runs <see cref="PlanService"/> against an in-memory SQLite database.</summary>
public sealed class PlanServiceTests : IDisposable
{
    private static readonly DateOnly Start = new(2026, 9, 28);

    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly PlanService _plans;

    public PlanServiceTests()
    {
        _connection.Open();
        _dbFactory = new TestDbFactory(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _plans = new PlanService(_dbFactory, new MenuGenerator(new Random(7)));

        using var db = _dbFactory.CreateDbContext();
        db.Database.EnsureCreated();

        // Replace the seed data with a known set: mains 1–5, each linked to sides 1–3.
        db.Mains.ExecuteDelete();
        db.Sides.ExecuteDelete();
        var sides = Enumerable.Range(1, 3).Select(i => new Side { Id = i, Name = $"Side {i}" }).ToList();
        db.Mains.AddRange(Enumerable.Range(1, 5).Select(i =>
            new Main { Id = i, Name = $"Main {i}", SideCount = 2, Sides = sides.ToList() }));
        db.SaveChanges();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task Generate_creates_consecutive_days_with_distinct_mains_and_sides()
    {
        await _plans.GenerateAsync(Start, 5);

        var plan = await _plans.GetPlanAsync();
        Assert.Equal(Enumerable.Range(0, 5).Select(Start.AddDays), plan.Select(d => d.Date));
        Assert.Equal(5, plan.Select(d => d.MainId).Distinct().Count());
        Assert.All(plan, d => Assert.Equal(2, d.Sides.Count));
    }

    [Fact]
    public async Task Generate_keeps_locked_days_and_does_not_reuse_their_mains()
    {
        await _plans.GenerateAsync(Start, 3);
        var locked = (await _plans.GetPlanAsync())[1];
        await _plans.SetLockedAsync(locked.Id, true);

        await _plans.GenerateAsync(Start, 5);

        var plan = await _plans.GetPlanAsync();
        var kept = Assert.Single(plan, d => d.Locked);
        Assert.Equal(locked.Id, kept.Id);
        Assert.Equal(locked.MainId, kept.MainId);
        Assert.Equal(5, plan.Select(d => d.MainId).Distinct().Count());
    }

    [Fact]
    public async Task Generate_drops_locked_days_outside_the_new_range()
    {
        await _plans.GenerateAsync(Start, 3);
        var first = (await _plans.GetPlanAsync())[0];
        await _plans.SetLockedAsync(first.Id, true);

        await _plans.GenerateAsync(Start.AddDays(1), 3);

        var plan = await _plans.GetPlanAsync();
        Assert.Equal(Start.AddDays(1), plan[0].Date);
        Assert.DoesNotContain(plan, d => d.Id == first.Id);
    }

    [Fact]
    public async Task RerollDay_picks_a_main_not_used_elsewhere_in_the_plan()
    {
        await _plans.GenerateAsync(Start, 3);
        var before = await _plans.GetPlanAsync();

        await _plans.RerollDayAsync(before[0].Id);

        var after = await _plans.GetPlanAsync();
        Assert.DoesNotContain(after[0].MainId, before.Select(d => d.MainId));
    }

    [Fact]
    public async Task SetMain_sets_the_main_and_picks_its_sides()
    {
        await _plans.GenerateAsync(Start, 1);
        var day = Assert.Single(await _plans.GetPlanAsync());

        await _plans.SetMainAsync(day.Id, 5);

        day = Assert.Single(await _plans.GetPlanAsync());
        Assert.Equal(5, day.MainId);
        Assert.Equal(2, day.Sides.Count);
    }

    [Fact]
    public async Task SetMain_null_empties_the_day()
    {
        await _plans.GenerateAsync(Start, 1);
        var day = Assert.Single(await _plans.GetPlanAsync());

        await _plans.SetMainAsync(day.Id, null);

        day = Assert.Single(await _plans.GetPlanAsync());
        Assert.Null(day.Main);
        Assert.Empty(day.Sides);
    }

    [Fact]
    public async Task Deleting_a_main_leaves_its_day_empty()
    {
        await _plans.GenerateAsync(Start, 1);
        var day = Assert.Single(await _plans.GetPlanAsync());

        await using (var db = await _dbFactory.CreateDbContextAsync())
            await db.Mains.Where(m => m.Id == day.MainId).ExecuteDeleteAsync();

        day = Assert.Single(await _plans.GetPlanAsync());
        Assert.Null(day.MainId);
    }

    [Fact]
    public async Task Generate_with_no_mains_creates_empty_days()
    {
        await using (var db = await _dbFactory.CreateDbContextAsync())
            await db.Mains.ExecuteDeleteAsync();

        await _plans.GenerateAsync(Start, 3);

        var plan = await _plans.GetPlanAsync();
        Assert.Equal(3, plan.Count);
        Assert.All(plan, d => Assert.Null(d.MainId));
    }

    private sealed class TestDbFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }
}
