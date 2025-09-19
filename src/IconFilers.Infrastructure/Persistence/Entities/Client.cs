using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class Client
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Contact { get; set; }

    public string? Contact2 { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateOnly? Dob { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? Status { get; set; }

    public virtual ICollection<ClientAssignment> ClientAssignments { get; set; } = new List<ClientAssignment>();

    public virtual ICollection<ClientDocument> ClientDocuments { get; set; } = new List<ClientDocument>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
