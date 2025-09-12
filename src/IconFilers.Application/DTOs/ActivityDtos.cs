// IconFilers.Application/DTOs/ActivityDtos.cs
namespace IconFilers.Application.DTOs;

public record ActivityDto(int Id, Guid? ActorId, string ActionType, string? TargetType, string? TargetId, string? Metadata, DateTime CreatedAt);
public record CreateActivityDto(Guid? ActorId, string ActionType, string? TargetType = null, string? TargetId = null, string? Metadata = null);
