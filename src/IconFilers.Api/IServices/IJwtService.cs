namespace IconFilers.Api.IServices
{
    public interface IJwtService
    {
        string GenerateToken(System.Guid userId, string email, string role);
    }
}
