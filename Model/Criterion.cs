using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Criterion
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<TaskCriterion> TaskCriteria { get; set; } = new List<TaskCriterion>();
}
