using System.ComponentModel.DataAnnotations;

namespace MenuPlanner.Data;

public class Main
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(500), Url]
    public string? RecipeUrl { get; set; }

    /// <summary>How many sides the generator picks for this main (0–3).</summary>
    [Range(0, 3)]
    public int SideCount { get; set; } = 1;

    /// <summary>The sides that go with this main.</summary>
    public List<Side> Sides { get; set; } = [];
}
