namespace IconFilers.Api.IServices
{
    public interface IJwtService
    {
        string GenerateToken(string email, string role);
    }
}
