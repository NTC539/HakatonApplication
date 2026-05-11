using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class HakatonSponsorsSummary
{
    public string? SponsorName { get; set; }

    public string? HakatonName { get; set; }

    public decimal? TotalContribution { get; set; }

    public string? SponsoredNominations { get; set; }
}
