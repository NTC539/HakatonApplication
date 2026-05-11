using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class StageType
{
    public int Id { get; set; }

    public string StageType1 { get; set; } = null!;

    public virtual ICollection<Stage> Stages { get; set; } = new List<Stage>();
}
