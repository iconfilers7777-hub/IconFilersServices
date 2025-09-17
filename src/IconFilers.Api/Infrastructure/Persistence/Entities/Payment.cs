using System;
using System.Collections.Generic;

namespace IconFilers.Api.Infrastructure.Persistence.Entities;

public partial class Payment
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public decimal Amount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Discount { get; set; }

    public decimal NetAmount { get; set; }

    public string PaymentMode { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;
}
