// IconFilers.Domain/Entities/User.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IconFilers.Domain.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(256)]
        public string Email { get; set; } = null!;

        [MaxLength(30)]
        public string? Phone { get; set; }

        [Required]
        public string PasswordHash { get; set; } = null!;

        public int? RoleId { get; set; }
        public Role? Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        // Navigation
        public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
        public ICollection<Client>? AssignedClients { get; set; }
        public ICollection<Lead>? AssignedLeads { get; set; }
        public ICollection<Payment>? AssignedPayments { get; set; }
    }
}
