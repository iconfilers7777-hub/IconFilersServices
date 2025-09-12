using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconFilers.Domain.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Permissions stored as JSON string. e.g. {"clients":["read","write"],...}
        /// </summary>
        [Required]
        public string Permissions { get; set; } = "{}";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
