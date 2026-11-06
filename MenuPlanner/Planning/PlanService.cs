using MenuPlanner.Data;
using Microsoft.EntityFrameworkCore;

namespace MenuPlanner.Planning;

/// <summary>Reads and changes the saved plan.</summary>
public sealed class PlanService(IDbContextFactory<AppDbContext> dbFactory, MenuGenerator generator)
{
    public const int MaxDays = 14;

    public async Task<List<PlanDay>> GetPlanAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.PlanDays
            .AsNoTracking()
            .Include(d => d.Main)
            .Include(d => d.Sides.OrderBy(s => s.Name))
            .OrderBy(d => d.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Replaces the plan with <paramref name="days"/> days starting at <paramref name="start"/>.
    /// Locked days inside the new range are kept as they are.
    /// </summary>
    public async Task GenerateAsync(DateOnly start, int days)
    {
        days = Math.Clamp(days, 1, MaxDays);
        var end = start.AddDays(days - 1);

        await using var db = await dbFactory.CreateDbContextAsync();
        var mains = await LoadMainsAsync(db);
        var existing = await db.PlanDays.Include(d => d.Sides).ToListAsync();

        var kept = existing.Where(d => d.Locked && d.Date >= start && d.Date <= end).ToList();
        db.PlanDays.RemoveRange(existing.Except(kept));

        var keptDates = kept.Select(d => d.Date).ToHashSet();
        var openDates = Enumerable.Range(0, days)
            .Select(start.AddDays)
            .Where(date => !keptDates.Contains(date))
            .ToList();

        var picks = generator.PickMains(Options(mains), openDates.Count, kept.MainIds());
        for (var i = 0; i < openDates.Count; i++)
        {
            var day = new PlanDay { Date = openDates[i] };
            if (i < picks.Count)
                Assign(day, mains[picks[i].Id]);
            db.PlanDays.Add(day);
        }

        await db.SaveChangesAsync();
    }

    /// <summary>Picks a different main (unused this plan, if possible) and new sides for one day.</summary>
    public async Task RerollDayAsync(int dayId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var mains = await LoadMainsAsync(db);
        var plan = await db.PlanDays.Include(d => d.Sides).ToListAsync();
        var day = plan.Single(d => d.Id == dayId);

        // Every other day's main plus the current one count as used, so the current main
        // only comes back when it's the least-used option left.
        var used = plan.Where(d => d.Id != dayId).MainIds();
        if (day.MainId is int current)
            used = used.Append(current);

        var pick = generator.PickMains(Options(mains), 1, used).SingleOrDefault();
        if (pick is null)
            Clear(day);
        else
            Assign(day, mains[pick.Id]);

        await db.SaveChangesAsync();
    }

    /// <summary>Keeps the day's main and re-picks its sides.</summary>
    public async Task RerollSidesAsync(int dayId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var day = await db.PlanDays.Include(d => d.Sides).SingleAsync(d => d.Id == dayId);
        if (day.MainId is not int mainId)
            return;

        var main = await db.Mains.Include(m => m.Sides).SingleAsync(m => m.Id == mainId);
        Assign(day, main);
        await db.SaveChangesAsync();
    }

    /// <summary>Sets a specific main for a day (or clears it when null) and picks its sides.</summary>
    public async Task SetMainAsync(int dayId, int? mainId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var day = await db.PlanDays.Include(d => d.Sides).SingleAsync(d => d.Id == dayId);

        if (mainId is null)
            Clear(day);
        else
            Assign(day, await db.Mains.Include(m => m.Sides).SingleAsync(m => m.Id == mainId));

        await db.SaveChangesAsync();
    }

    public async Task SetLockedAsync(int dayId, bool locked)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await db.PlanDays
            .Where(d => d.Id == dayId)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.Locked, locked));
    }

    private static async Task<Dictionary<int, Main>> LoadMainsAsync(AppDbContext db) =>
        await db.Mains.Include(m => m.Sides).ToDictionaryAsync(m => m.Id);

    private static List<MainOption> Options(Dictionary<int, Main> mains) =>
        mains.Values
            .Select(m => new MainOption(m.Id, m.SideCount, m.Sides.Select(s => s.Id).ToList()))
            .ToList();

    private void Assign(PlanDay day, Main main)
    {
        var option = new MainOption(main.Id, main.SideCount, main.Sides.Select(s => s.Id).ToList());
        var sideIds = generator.PickSides(option);

        day.Main = main;
        day.MainId = main.Id;
        day.Sides.Clear();
        day.Sides.AddRange(main.Sides.Where(s => sideIds.Contains(s.Id)));
    }

    private static void Clear(PlanDay day)
    {
        day.Main = null;
        day.MainId = null;
        day.Sides.Clear();
    }
}

internal static class PlanDayExtensions
{
    public static IEnumerable<int> MainIds(this IEnumerable<PlanDay> days) =>
        days.Where(d => d.MainId is not null).Select(d => d.MainId!.Value);
}
