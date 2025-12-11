// IconFilers.Application/DTOs/ClientDocumentDtos.cs
namespace IconFilers.Application.DTOs;
public class ClientDocumentDto
{
    public int Id { get; set; }
    public string ClientId { get; set; }
    public string DocumentType { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
    public string Status { get; set; } = null!;

    public string? Type { get; set; }
}
