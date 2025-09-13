using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class PaymentMode
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
