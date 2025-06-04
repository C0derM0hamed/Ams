public interface IJwtHelper
{
    string GenerateToken(Guid userId, string role);
}
