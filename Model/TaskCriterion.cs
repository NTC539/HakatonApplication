using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class TaskCriterion
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int CriteriaId { get; set; }

    public string? Description { get; set; }

    public decimal? MaxMark { get; set; }

    public virtual Criterion Criteria { get; set; } = null!;

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();

    public virtual StageTask Task { get; set; } = null!;
}
