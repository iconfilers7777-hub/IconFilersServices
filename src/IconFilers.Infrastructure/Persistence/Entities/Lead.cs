using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class Lead
{
    public int Id { get; set; }

    public int? ClientId { get; set; }

    public string? Name { get; set; }

    public string? Contact { get; set; }

    public string? Email { get; set; }

    public string? Source { get; set; }

    public int? AssignedTeamId { get; set; }

    public Guid? AssignedTo { get; set; }

    public string? Status { get; set; }

    public string? Stage { get; set; }

    public DateTime? ConversionDate { get; set; }

    public DateTime? LastContactedAt { get; set; }

    public string? Notes { get; set; }

    public bool IsCitizen { get; set; }

    public bool IsDuplicate { get; set; }

    public bool IsVoicemail { get; set; }

    public int ServiceEmailsSent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<LeadNote> LeadNotes { get; set; } = new List<LeadNote>();
}
