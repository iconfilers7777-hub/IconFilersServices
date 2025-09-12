// IconFilers.Domain/Entities/Client.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IconFilers.Domain.Entities
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string? ClientCode { get; set; }

        [MaxLength(150)]
        public string? FirstName { get; set; }

        [MaxLength(150)]
        public string? LastName { get; set; }

        [MaxLength(250)]
        public string? BusinessName { get; set; }

        [MaxLength(256)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(20)]
        public string? Pan { get; set; }

        [MaxLength(32)]
        public string? Gstin { get; set; }

        public int? AssignedTeamId { get; set; }
        public Team? AssignedTeam { get; set; }

        public Guid? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(100)]
        public string? Source { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Additional fields you asked to add
        public string? Address { get; set; }
        public DateTime? Dob { get; set; }

        public ICollection<Lead>? Leads { get; set; }
        public ICollection<Referral>? Referrals { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<ClientDocument>? ClientDocuments { get; set; }
    }
}
