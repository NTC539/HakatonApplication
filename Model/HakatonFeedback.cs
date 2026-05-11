using System;
using System.Collections.Generic;

namespace HakatonApplication.Models;

public partial class HakatonFeedback
{
    public int Id { get; set; }

    public int HakatonId { get; set; }

    public int UserId { get; set; }

    public decimal? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Hakaton Hakaton { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
