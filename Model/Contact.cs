using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Contact
{
    public int Id { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();

    public virtual ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();

    public virtual User? User { get; set; }
}
