using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class StageTask
{
    public int Id { get; set; }

    public int StageId { get; set; }

    public string? Description { get; set; }

    public short? IsSolutionsPublic { get; set; }

    public virtual Stage Stage { get; set; } = null!;

    public virtual ICollection<TaskCriterion> TaskCriteria { get; set; } = new List<TaskCriterion>();

    public virtual ICollection<Solution> Solutions { get; set; } = new List<Solution>();
}
