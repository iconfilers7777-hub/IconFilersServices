using System;
using System.Collections.Generic;

namespace IconFilers.Infrastructure.Persistence.Entities;

public partial class ClientDocument
{
    public int Id { get; set; }

    public string ClientId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public string Status { get; set; } = null!;   

    public string? Type { get; set; }

    public virtual Client Client { get; set; } = null!;
}
