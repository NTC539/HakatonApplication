using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class HakatonWinner
{
    public int HakatonNominationId { get; set; }

    public int TeamId { get; set; }

    public string? Description { get; set; }

    public virtual HakatonNomination HakatonNomination { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}
