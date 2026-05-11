using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class PrizeFund
{
    public int Id { get; set; }

    public int HakatonNominationId { get; set; }

    public int Place { get; set; }

    public virtual HakatonNomination HakatonNomination { get; set; } = null!;

    public virtual ICollection<SponsorContribution> Contributions { get; set; } = new List<SponsorContribution>();
}
