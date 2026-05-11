using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class SponsorContribution
{
    public int Id { get; set; }

    public int HakatonId { get; set; }

    public int? SponsorId { get; set; }

    public decimal? Money { get; set; }

    public string? Additional { get; set; }

    public string? Description { get; set; }

    public virtual Hakaton Hakaton { get; set; } = null!;

    public virtual Sponsor? Sponsor { get; set; }

    public virtual ICollection<PrizeFund> Prizes { get; set; } = new List<PrizeFund>();
}
