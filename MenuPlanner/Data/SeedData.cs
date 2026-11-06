namespace MenuPlanner.Data;

/// <summary>Sample dishes inserted by the initial migration.</summary>
internal static class SeedData
{
    public static readonly Side[] Sides =
    [
        new() { Id = 1, Name = "Rice" },
        new() { Id = 2, Name = "Roasted broccoli" },
        new() { Id = 3, Name = "Mashed potatoes" },
        new() { Id = 4, Name = "Green salad" },
        new() { Id = 5, Name = "Garlic bread" },
        new() { Id = 6, Name = "Corn on the cob" },
        new() { Id = 7, Name = "Chips and salsa" },
    ];

    public static readonly Main[] Mains =
    [
        new() { Id = 1, Name = "Chicken stir-fry", SideCount = 1 },
        new() { Id = 2, Name = "Meatloaf", SideCount = 2 },
        new() { Id = 3, Name = "Spaghetti and meatballs", SideCount = 2 },
        new() { Id = 4, Name = "Tacos", SideCount = 1 },
        new() { Id = 5, Name = "Grilled burgers", SideCount = 2 },
        new() { Id = 6, Name = "Lasagna", SideCount = 0 },
        new() { Id = 7, Name = "Baked salmon", SideCount = 2 },
    ];

    public static readonly object[] MainSides =
    [
        Link(1, 1), Link(1, 2),
        Link(2, 3), Link(2, 2), Link(2, 6),
        Link(3, 4), Link(3, 5),
        Link(4, 1), Link(4, 7),
        Link(5, 6), Link(5, 4), Link(5, 7),
        Link(6, 4), Link(6, 5),
        Link(7, 1), Link(7, 2), Link(7, 4),
    ];

    private static object Link(int mainId, int sideId) => new { MainsId = mainId, SidesId = sideId };
}
