using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class User
{
    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Patronymic { get; set; }

    public int? ContactId { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public short? IsPublic { get; set; }
    public short? IsAdmin { get; set; }

    public string? Password { get; set; }

    public string? Salt { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual ICollection<HakatonFeedback> HakatonFeedbacks { get; set; } = new List<HakatonFeedback>();

    public virtual ICollection<HakatonRegistration> HakatonRegistrations { get; set; } = new List<HakatonRegistration>();

    public virtual ICollection<SolutionFeedback> SolutionFeedbacks { get; set; } = new List<SolutionFeedback>();
}
