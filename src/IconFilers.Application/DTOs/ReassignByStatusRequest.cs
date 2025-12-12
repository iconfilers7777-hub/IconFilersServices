using System;

namespace IconFilers.Application.DTOs
{
    public class ReassignByStatusRequest
    {
        public ClientStatus Status { get; set; }
        public Guid AssignedTo { get; set; }
    }
}
