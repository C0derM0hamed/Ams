namespace AmsApi.Interfaces
{
    public interface IUserService
    {
        Task<RegisterResponse> RegisterUserAsync(CreateUserDto dto);
        Task<AuthResponse> LoginAsync(LoginDto dto);
        Task<RegisterResponse> AutoRegisterAsync(Guid id);
    }
}
