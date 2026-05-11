using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class TeamHistory
{
    public int Id { get; set; }

    public int? RegistrationId { get; set; }

    public string? Action { get; set; }

    public int? OldTeamId { get; set; }

    public int? NewTeamId { get; set; }

    public DateTime? ChangedAt { get; set; }
}
