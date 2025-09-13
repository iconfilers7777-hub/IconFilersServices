using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class Team
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? LeadId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? Lead { get; set; }

    public virtual ICollection<TeamTarget> TeamTargets { get; set; } = new List<TeamTarget>();
}
