using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class Invoice
{
    public Guid Id { get; set; }

    public int? ClientId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Client? Client { get; set; }

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
