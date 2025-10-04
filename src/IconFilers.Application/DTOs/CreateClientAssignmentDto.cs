using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Application.DTOs
{
    public class CreateClientAssignmentDto
    {
        public int ClientId { get; set; }
        public Guid AssignedTo { get; set; }
        public Guid AssignedBy { get; set; }
        public string RoleAtAssignment { get; set; } = null!;
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
