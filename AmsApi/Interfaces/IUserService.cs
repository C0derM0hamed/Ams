namespace AmsApi.Interfaces
{
    public interface IUserService
    {
        Task<string> RegisterUserAsync(CreateUserDto dto);
        Task<AuthResponse> LoginAsync(LoginDto dto);
    }
}
