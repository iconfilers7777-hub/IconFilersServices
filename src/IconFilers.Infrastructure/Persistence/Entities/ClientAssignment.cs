using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class ClientAssignment
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public Guid AssignedTo { get; set; }

    public Guid AssignedBy { get; set; }

    public string? RoleAtAssignment { get; set; }

    public DateTime AssignedAt { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public virtual User AssignedByNavigation { get; set; } = null!;

    public virtual User AssignedToNavigation { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;
}
