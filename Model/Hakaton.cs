using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class Hakaton
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<HakatonFeedback> HakatonFeedbacks { get; set; } = new List<HakatonFeedback>();

    public virtual ICollection<HakatonNomination> HakatonNominations { get; set; } = new List<HakatonNomination>();

    public virtual ICollection<HakatonRegistration> HakatonRegistrations { get; set; } = new List<HakatonRegistration>();

    public virtual ICollection<SponsorContribution> SponsorContributions { get; set; } = new List<SponsorContribution>();

    public virtual ICollection<Stage> Stages { get; set; } = new List<Stage>();

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    public virtual ICollection<HakatonType> HakatonTypes { get; set; } = new List<HakatonType>();
}
