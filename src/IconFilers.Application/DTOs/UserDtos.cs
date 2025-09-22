// IconFilers.Application/DTOs/UserDtos.cs
namespace IconFilers.Application.DTOs
{
    public record UserDto(
            Guid Id,
            string FirstName,
            string LastName,
            string Email,
            string Phone,
            string? DeskNumber,
            string? WhatsAppNumber,
            string Role,
            Guid? ReportsTo,
            string? TeamName,
            decimal? TargetAmount,
            decimal? DiscountAmount,
            DateTime CreatedAt
        );
}
