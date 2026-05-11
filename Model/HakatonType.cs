using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class HakatonType
{
    public int Id { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Hakaton> Hakatons { get; set; } = new List<Hakaton>();
}
