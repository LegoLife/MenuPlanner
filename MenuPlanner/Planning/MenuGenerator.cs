namespace MenuPlanner.Planning;

/// <summary>A main the generator can choose, with the sides that go with it.</summary>
public sealed record MainOption(int Id, int SideCount, IReadOnlyList<int> SideIds);

/// <summary>
/// Random dinner picking. Pure logic with no database access, so it can be tested directly.
/// </summary>
public sealed class MenuGenerator(Random random)
{
    /// <summary>
    /// Picks <paramref name="count"/> mains. It always picks from the least-used mains, so there
    /// are no repeats while unused mains remain, and repeats are spread evenly after that.
    /// </summary>
    /// <param name="alreadyUsed">Mains already in the plan (e.g. on locked days); they count as used.</param>
    public IReadOnlyList<MainOption> PickMains(
        IReadOnlyList<MainOption> mains, int count, IEnumerable<int> alreadyUsed)
    {
        if (mains.Count == 0)
            return [];

        var uses = mains.ToDictionary(m => m.Id, _ => 0);
        foreach (var id in alreadyUsed)
        {
            if (uses.ContainsKey(id))
                uses[id]++;
        }

        var picks = new List<MainOption>(count);
        for (var i = 0; i < count; i++)
        {
            var fewest = uses.Values.Min();
            var candidates = mains.Where(m => uses[m.Id] == fewest).ToList();
            var pick = candidates[random.Next(candidates.Count)];
            uses[pick.Id]++;
            picks.Add(pick);
        }

        return picks;
    }

    /// <summary>
    /// Picks the main's <see cref="MainOption.SideCount"/> sides at random from its linked sides,
    /// or all of them if it has fewer.
    /// </summary>
    public IReadOnlyList<int> PickSides(MainOption main)
    {
        var take = Math.Min(main.SideCount, main.SideIds.Count);
        return main.SideIds.OrderBy(_ => random.Next()).Take(take).ToList();
    }
}
