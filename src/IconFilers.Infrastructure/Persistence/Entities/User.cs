using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!; // kept for compatibility but will store hashed password

    [Required(ErrorMessage = "Phone is required")]
    [RegularExpression(@"^(?:(?:\+1[- ]?)?\(?[2-9][0-9]{2}\)?[- ]?[0-9]{3}[- ]?[0-9]{4}|(?:\+91[- ]?)?[6-9][0-9]{9})$", ErrorMessage = "Invalid phone number. Acceptable formats: US or India numbers (e.g. +1 555-555-5555 or +91 9123456789)")]
    public string Phone { get; set; } = null!;

    public string? DeskNumber { get; set; }

    [RegularExpression(@"^(?:$|(?:(?:\+1[- ]?)?\(?[2-9][0-9]{2}\)?[- ]?[0-9]{3}[- ]?[0-9]{4}|(?:\+91[- ]?)?[6-9][0-9]{9}))$", ErrorMessage = "Invalid WhatsApp number. Acceptable formats: US or India numbers or empty")]
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
