using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Nomination
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<HakatonNomination> HakatonNominations { get; set; } = new List<HakatonNomination>();
}
