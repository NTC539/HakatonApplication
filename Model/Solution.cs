using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Solution
{
    public int Id { get; set; }

    public int TeamId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string? Source { get; set; }

    public virtual ICollection<SolutionFeedback> SolutionFeedbacks { get; set; } = new List<SolutionFeedback>();

    public virtual Team Team { get; set; } = null!;

    public virtual ICollection<StageTask> Tasks { get; set; } = new List<StageTask>();
}
