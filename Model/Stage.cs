using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Stage
{
    public int Id { get; set; }

    public int HakatonId { get; set; }

    public int? OrderNumber { get; set; }

    public int? StageTypeId { get; set; }

    public string? Description { get; set; }

    public int? LocationId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual Hakaton Hakaton { get; set; } = null!;

    public virtual Location? Location { get; set; }

    public virtual StageType? StageType { get; set; }

    public virtual ICollection<StageTask> Tasks { get; set; } = new List<StageTask>();
}
