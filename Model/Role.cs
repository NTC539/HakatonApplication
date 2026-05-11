using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<HakatonRegistration> HakatonRegistrations { get; set; } = new List<HakatonRegistration>();
}
