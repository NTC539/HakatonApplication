using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Location
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public int? ContactId { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual ICollection<Stage> Stages { get; set; } = new List<Stage>();
}
