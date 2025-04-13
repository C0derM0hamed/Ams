namespace AmsApi.Interfaces;

public interface IInstructorService
{
    Task<Instructor> RegisterAsync(RegisterInstructorDto dto);
    Task<AuthResponse> LoginAsync(LoginInstructorDto dto);
    Task<Instructor?> UpdateAsync(int id, UpdateInstructorDto dto);
    Task<Instructor?> GetByIdAsync(int id);
    Task<bool> DeleteAsync(int id);
}