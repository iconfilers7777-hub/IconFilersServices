// IconFilers.Application/DTOs/ReferralDtos.cs
using System;

namespace IconFilers.Application.DTOs
{
    public record ReferralDto(
        int Id,
        int ClientId,
        string? ReferrerName,
        string? ReferrerContact,
        string? CommissionStatus,
        DateTime CreatedAt
    );

    public record CreateReferralDto(
        int ClientId,
        string? ReferrerName = null,
        string? ReferrerContact = null,
        string? CommissionStatus = null
    );

    public record UpdateReferralDto(
        int Id,
        string? ReferrerName = null,
        string? ReferrerContact = null,
        string? CommissionStatus = null
    );
}
