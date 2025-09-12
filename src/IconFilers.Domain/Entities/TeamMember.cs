// IconFilers.Domain/Entities/TeamMember.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconFilers.Domain.Entities
{
    public class TeamMember
    {
        [Key, Column(Order = 0)]
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        [Key, Column(Order = 1)]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [MaxLength(100)]
        public string? RoleInTeam { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
