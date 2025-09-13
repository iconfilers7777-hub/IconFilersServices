using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class LeadNote
{
    public Guid Id { get; set; }

    public int LeadId { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Lead Lead { get; set; } = null!;
}
