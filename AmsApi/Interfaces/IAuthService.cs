namespace AmsApi.Interfaces
{
    public interface IAuthService
    {
        AuthResponse Authenticate(LoginDto loginDto);
    }
}
