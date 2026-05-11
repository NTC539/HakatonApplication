using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class OverdueTeam
{
    public string? Team { get; set; }

    public string? Hakaton { get; set; }

    public string? Solution { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public decimal? Overdue { get; set; }
}
