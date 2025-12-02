using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!; // kept for compatibility but will store hashed password
    public string Phone { get; set; } = null!;

    public string? DeskNumber { get; set; }

    public string? WhatsAppNumber { get; set; }

    public string Role { get; set; } = null!;

    public Guid? ReportsTo { get; set; }

    public string? TeamName { get; set; }

    public decimal? TargetAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ClientAssignment> ClientAssignmentAssignedByNavigations { get; set; } = new List<ClientAssignment>();

    public virtual ICollection<ClientAssignment> ClientAssignmentAssignedToNavigations { get; set; } = new List<ClientAssignment>();
}
