// IconFilers.Domain/Entities/PaymentMode.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IconFilers.Domain.Entities
{
    public class PaymentMode
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        // Navigation
        public ICollection<Payment>? Payments { get; set; }
    }
}
