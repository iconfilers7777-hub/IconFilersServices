using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Infrastructure.Persistence.Entities
{
    public class EmployeeModel
    {
        public Guid? Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? DeskNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Role { get; set; }

        public Guid? ReportsTo { get; set; }   // Nullable if no manager

        public string? TeamName { get; set; }

        public decimal? TargetAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
