using System;
using System.Collections.Generic;

namespace IconFilers.Api.Infrastructure.Persistence.Entities;

public partial class Invoice
{
    public Guid Id { get; set; }

    public int ClientId { get; set; }

    public string Description { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;
}
