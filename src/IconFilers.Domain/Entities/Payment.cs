// IconFilers.Domain/Entities/Payment.cs
using IconFilers.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconFilers.Domain.Entities
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "INR";

        public DateTime PaymentDate { get; set; }

        // Replaced 'Method' string column with a FK to PaymentMode
        public int? PaymentModeId { get; set; }
        public PaymentMode? PaymentMode { get; set; }

        [MaxLength(200)]
        public string? Reference { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public Guid? AssignedTo { get; set; }
        public User? AssignedUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Additional payment fields requested
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } = 0m;

        // NetAmount is a persisted computed column in DB: [Amount] + [TaxAmount] - [Discount]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; private set; }
    }
}
