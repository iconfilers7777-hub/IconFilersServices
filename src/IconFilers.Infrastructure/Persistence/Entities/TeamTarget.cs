using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class TeamTarget
{
    public int Id { get; set; }

    public int TeamId { get; set; }

    public int Year { get; set; }

    public byte? Month { get; set; }

    public decimal TargetAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Team Team { get; set; } = null!;
}
