using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconFilers.Application.DTOs
{
    public class ClientAssignmentDto
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public Guid AssignedTo { get; set; }
        public Guid AssignedBy { get; set; }
        public string RoleAtAssignment { get; set; } = null!;
        public DateTime AssignedAt { get; set; }
        public ClientStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
