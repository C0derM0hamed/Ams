namespace AmsApi.Interfaces;

public interface IAttendeeService
{
    Task<Attendee> RegisterAsync(RegisterAttendeeDto dto);
    Task<AuthResponse> LoginAsync(LoginAttendeeDto dto);
    Task<bool> DeleteAsync(int id);
    Task<Attendee?> UpdateAsync(int id, UpdateAttendeeDto dto);
    Task<Attendee?> GetByIdAsync(int id);


}
