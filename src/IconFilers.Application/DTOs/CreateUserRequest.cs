using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Application.DTOs
{
    public class CreateUserRequest
    {
        [Required] public string FirstName { get; set; } = null!;
        [Required] public string LastName { get; set; } = null!;
        [Required][EmailAddress] public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Required] public string Phone { get; set; } = null!;
        public string? DeskNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        [Required] public string Role { get; set; } = null!;
        public Guid? ReportsTo { get; set; }
        public string? TeamName { get; set; }
        public decimal? TargetAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}
