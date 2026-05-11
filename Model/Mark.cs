using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Mark
{
    public int TaskCriteriaId { get; set; }

    public int TeamId { get; set; }

    public int RegistrationId { get; set; }

    public decimal? Mark1 { get; set; }

    public string? Description { get; set; }

    public virtual HakatonRegistration Registration { get; set; } = null!;

    public virtual TaskCriterion TaskCriteria { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}
