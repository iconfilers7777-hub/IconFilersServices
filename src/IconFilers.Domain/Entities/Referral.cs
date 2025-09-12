// IconFilers.Domain/Entities/Referral.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace IconFilers.Domain.Entities
{
    public class Referral
    {
        [Key]
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        [MaxLength(250)]
        public string? ReferrerName { get; set; }

        [MaxLength(50)]
        public string? ReferrerContact { get; set; }

        [MaxLength(50)]
        public string? CommissionStatus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
