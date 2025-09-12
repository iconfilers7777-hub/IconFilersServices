// IconFilers.Domain/Entities/Activity.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace IconFilers.Domain.Entities
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }

        public Guid? ActorId { get; set; }
        public User? Actor { get; set; }

        [Required, MaxLength(100)]
        public string ActionType { get; set; } = null!;

        [MaxLength(100)]
        public string? TargetType { get; set; }

        /// <summary>
        /// Flexible target id stored as string (can store int or guid)
        /// </summary>
        [MaxLength(100)]
        public string? TargetId { get; set; }

        /// <summary>
        /// JSON metadata
        /// </summary>
        public string? Metadata { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
