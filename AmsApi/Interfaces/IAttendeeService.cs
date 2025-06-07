namespace AmsApi.Interfaces
{
    public interface IAttendeeService
    {
        Task<AttendeeDetailsDto> GetByIdAsync(Guid id);
        Task<Attendee> CreateAsync(CreateAttendeeDto dto , Guid adminId);
        Task<Attendee> UpdateAsync(Guid id, UpdateAttendeeDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<List<AttendeeSummaryDto>> GetAllAsync();
        Task UploadImageAsync(Guid attendeeId, byte[] imageBytes);
        Task<bool> AddSubjectToAttendee(Guid attendeeId, Guid subjectId);
        Task<Subject> GetSubjectForAttendee(Guid attendeeId, Guid subjectId);
        Task<Attendee> GetByEmailAsync(string email);
        Task<bool> RemoveSubjectFromAttendee(Guid attendee_id, Guid subject_id);
        Task<List<Subject>> GetSubjectsForAttendeeAsync(Guid attendeeId);
    }
}
