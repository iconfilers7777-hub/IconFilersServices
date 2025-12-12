using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Application.DTOs
{
    public class UpdateClientAssignmentDto
    {
        public string? RoleAtAssignment { get; set; }
        public ClientStatus? Status { get; set; }
        public string? Notes { get; set; }
        // Optional fields to support reassignment by admin
        public Guid? AssignedTo { get; set; }
        // AssignedBy will be set by the API (controller) from the current user claims when performing a reassignment
        public Guid? AssignedBy { get; set; }
    }
}
