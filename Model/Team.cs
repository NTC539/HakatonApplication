using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Team
{
    public int Id { get; set; }

    public int HakatonId { get; set; }

    public string? Name { get; set; }

    public virtual Hakaton Hakaton { get; set; } = null!;

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();

    public virtual ICollection<Solution> Solutions { get; set; } = new List<Solution>();

    public virtual ICollection<HakatonRegistration> Registrations { get; set; } = new List<HakatonRegistration>();
}
