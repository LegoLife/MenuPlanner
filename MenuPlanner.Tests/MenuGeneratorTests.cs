using MenuPlanner.Planning;

namespace MenuPlanner.Tests;

public class MenuGeneratorTests
{
    public static TheoryData<int> Seeds => [1, 2, 3, 42, 1234];

    private static List<MainOption> Mains(int count) =>
        Enumerable.Range(1, count).Select(id => new MainOption(id, 1, [])).ToList();

    [Theory, MemberData(nameof(Seeds))]
    public void PickMains_does_not_repeat_while_unused_mains_remain(int seed)
    {
        var picks = new MenuGenerator(new Random(seed)).PickMains(Mains(7), 7, []);

        Assert.Equal(7, picks.Select(p => p.Id).Distinct().Count());
    }

    [Theory, MemberData(nameof(Seeds))]
    public void PickMains_avoids_already_used_mains(int seed)
    {
        var picks = new MenuGenerator(new Random(seed)).PickMains(Mains(5), 3, [1, 2]);

        Assert.Equal([3, 4, 5], picks.Select(p => p.Id).Order());
    }

    [Theory, MemberData(nameof(Seeds))]
    public void PickMains_spreads_repeats_evenly_when_there_are_more_days_than_mains(int seed)
    {
        var picks = new MenuGenerator(new Random(seed)).PickMains(Mains(3), 7, []);

        var counts = picks.GroupBy(p => p.Id).Select(g => g.Count()).ToList();
        Assert.Equal(7, picks.Count);
        Assert.Equal(3, counts.Count);
        Assert.True(counts.Max() - counts.Min() <= 1);
    }

    [Fact]
    public void PickMains_returns_nothing_when_there_are_no_mains()
    {
        Assert.Empty(new MenuGenerator(new Random(1)).PickMains([], 7, []));
    }

    [Fact]
    public void PickMains_ignores_used_ids_that_are_not_mains()
    {
        var picks = new MenuGenerator(new Random(1)).PickMains(Mains(2), 2, [99]);

        Assert.Equal([1, 2], picks.Select(p => p.Id).Order());
    }

    [Theory, MemberData(nameof(Seeds))]
    public void PickSides_picks_side_count_distinct_linked_sides(int seed)
    {
        var main = new MainOption(1, 2, [10, 11, 12, 13]);

        var sides = new MenuGenerator(new Random(seed)).PickSides(main);

        Assert.Equal(2, sides.Distinct().Count());
        Assert.All(sides, id => Assert.Contains(id, main.SideIds));
    }

    [Fact]
    public void PickSides_uses_all_linked_sides_when_there_are_fewer_than_side_count()
    {
        var sides = new MenuGenerator(new Random(1)).PickSides(new MainOption(1, 3, [10, 11]));

        Assert.Equal([10, 11], sides.Order());
    }

    [Fact]
    public void PickSides_picks_none_when_side_count_is_zero()
    {
        Assert.Empty(new MenuGenerator(new Random(1)).PickSides(new MainOption(1, 0, [10, 11])));
    }
}
