using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class SolutionFeedback
{
    public int Id { get; set; }

    public int SolutionId { get; set; }

    public int UserId { get; set; }

    public decimal? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Solution Solution { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
