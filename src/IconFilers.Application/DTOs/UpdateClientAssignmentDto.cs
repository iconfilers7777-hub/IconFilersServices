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
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
