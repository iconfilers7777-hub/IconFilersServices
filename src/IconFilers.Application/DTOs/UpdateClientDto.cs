namespace IconFilers.Application.DTOs;

public class UpdateClientDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Status { get; set; }
    // Optional: set assigned user for the client (will create a new ClientAssignment)
    public Guid? AssignedTo { get; set; }
    // Add other properties as needed, all nullable
}
