// IconFilers.Domain/Entities/Team.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IconFilers.Domain.Entities
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        /// <summary>
        /// optional link to a user who is team lead
        /// </summary>
        public Guid? LeadId { get; set; }
        public User? Lead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
        public ICollection<Client>? Clients { get; set; }
        public ICollection<Lead>? Leads { get; set; }
    }
}
