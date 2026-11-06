using System.ComponentModel.DataAnnotations;

namespace MenuPlanner.Data;

public class Side
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public List<Main> Mains { get; set; } = [];

    public List<PlanDay> PlanDays { get; set; } = [];
}
