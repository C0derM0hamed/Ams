namespace AmsApi.Interfaces
{
    public interface ISubjectService
    {
        Task<List<SubjectListDto>> GetAllAsync();
        Task<SubjectDetailsDto?> GetByIdAsync(Guid id);
        Task<Subject> CreateAsync(CreateSubjectDto dto);
        Task<Subject?> UpdateAsync(Guid id, UpdateSubjectDto dto);
        Task<bool> DeleteAsync(Guid id);

        Task<List<AttendeeWithImageDto>> GetAttendeesAsync(Guid subjectId);
        Task<SubjectDate> AddSubjectDateAsync(Guid subjectId, CreateSubjectDateDto dto);
        Task<bool> RemoveSubjectDateAsync(Guid subjectId, Guid subjectDateId);
        Task<int> DeleteAllAsync();
    }
}
