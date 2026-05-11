using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class HakatonRegistration
{
    public int Id { get; set; }

    public int HakatonId { get; set; }

    public int UserId { get; set; }

    public int? RoleId { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public virtual Hakaton Hakaton { get; set; } = null!;

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();

    public virtual Role? Role { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
