using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class ClientDocument
{
    public Guid Id { get; set; }

    public int? ClientId { get; set; }

    public Guid? UploadedBy { get; set; }

    public string? StoragePath { get; set; }

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public string? DocType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Client? Client { get; set; }

    public virtual User? UploadedByNavigation { get; set; }
}
