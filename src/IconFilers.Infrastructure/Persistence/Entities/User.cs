using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string? Email { get; set; }

    public string? DisplayName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ClientDocument> ClientDocuments { get; set; } = new List<ClientDocument>();

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
