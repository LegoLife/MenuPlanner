namespace MenuPlanner.Data;

/// <summary>One dinner in the current plan.</summary>
public class PlanDay
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    /// <summary>Null when nothing is planned, e.g. the main was deleted.</summary>
    public int? MainId { get; set; }
    public Main? Main { get; set; }

    public List<Side> Sides { get; set; } = [];

    /// <summary>Locked days are skipped by "Regenerate all".</summary>
    public bool Locked { get; set; }
}
