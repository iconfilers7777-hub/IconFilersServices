// IconFilers.Application/DTOs/ClientDocumentDtos.cs
namespace IconFilers.Application.DTOs;

public record ClientDocumentDto(int Id, int ClientId, string Filename, string? Mime, string? StoragePath, Guid? UploadedBy, DateTime UploadedAt);
public record CreateClientDocumentDto(int ClientId, string Filename, string? Mime, string? StoragePath, Guid? UploadedBy);
