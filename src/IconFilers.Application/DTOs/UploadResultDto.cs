namespace IconFilers.Application.DTOs;

public class UploadResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ClientDocumentDto? Document { get; set; }
}
