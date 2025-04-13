namespace AmsApi.Interfaces;

public interface ISubjectService
{
    Task<IEnumerable<SubjectDto>> GetAllAsync();
    Task<SubjectDto?> GetByIdAsync(int id);
    Task<SubjectDto> CreateAsync(CreateSubjectDto dto);
    Task<bool> UpdateAsync(int id, UpdateSubjectDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> AddAttendeeToSubject(int subjectId, int attendeeId);
    Task<bool> RemoveAttendeeFromSubject(int subjectId, int attendeeId);
    Task<bool> AddInstructorToSubject(int subjectId, int instructorId);
    Task<bool> RemoveInstructorFromSubject(int subjectId, int instructorId);

}
