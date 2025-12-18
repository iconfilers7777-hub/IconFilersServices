using System.Collections.Generic;

namespace IconFilers.Application.DTOs
{
    public class MyAssignmentsDto
    {
        public IEnumerable<ClientDto> Clients { get; set; } = new List<ClientDto>();
        public IEnumerable<LeadDto> Leads { get; set; } = new List<LeadDto>();
    }
}
