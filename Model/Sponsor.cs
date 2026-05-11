using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Sponsor
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ContactId { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual ICollection<SponsorContribution> SponsorContributions { get; set; } = new List<SponsorContribution>();
}
