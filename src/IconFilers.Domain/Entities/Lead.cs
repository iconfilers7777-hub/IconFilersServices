// IconFilers.Domain/Entities/Lead.cs
using System;
using System.ComponentModel.DataAnnotations;
using IconFilers.Domain.Enums;

namespace IconFilers.Domain.Entities
{
    public class Lead
    {
        [Key]
        public int Id { get; set; }

        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Contact { get; set; }

        [MaxLength(256)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Source { get; set; }

        public int? AssignedTeamId { get; set; }
        public Team? AssignedTeam { get; set; }

        public Guid? AssignedTo { get; set; }
        public User? AssignedUser { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public DateTime? LastContactedAt { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Additional fields
        [MaxLength(100)]
        public string? Stage { get; set; }

        public DateTime? ConversionDate { get; set; }
    }
}
