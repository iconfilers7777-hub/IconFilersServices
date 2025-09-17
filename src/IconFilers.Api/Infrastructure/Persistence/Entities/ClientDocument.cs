using System;
using System.Collections.Generic;

namespace IconFilers.Api.Infrastructure.Persistence.Entities;

public partial class ClientDocument
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public virtual Client Client { get; set; } = null!;
}
