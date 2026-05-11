using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class HakatonNomination
{
    public int Id { get; set; }

    public int HakatonId { get; set; }

    public int NominationId { get; set; }

    public string? Description { get; set; }

    public virtual Hakaton Hakaton { get; set; } = null!;

    public virtual Nomination Nomination { get; set; } = null!;

    public virtual ICollection<PrizeFund> PrizeFunds { get; set; } = new List<PrizeFund>();
}
