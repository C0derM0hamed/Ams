namespace AmsApi.Interfaces;

public interface IInstructorService
{
    Task<List<Instructor>> GetAllAsync();
    Task<Instructor?> GetByIdAsync(Guid id);
    Task<Instructor?> GetByEmailAsync(string email);
    Task<Instructor> CreateAsync(CreateInstructorDto dto);
    Task<Instructor?> UpdateAsync(Guid id, UpdateInstructorDto dto);
    Task<bool> DeleteAsync(Guid id);

    Task<List<Subject>> GetSubjectsForInstructorAsync(Guid instructorId);
    Task<Subject?> GetSubjectForInstructorAsync(Guid instructorId, Guid subjectId);
    Task<bool> AssignSubjectToInstructorAsync(Guid instructorId, Guid subjectId);
    Task<bool> RemoveSubjectFromInstructorAsync(Guid instructorId, Guid subjectId);

    /// <summary>
    /// Saves the uploaded image and returns the file path under wwwroot/instructors/{id}/image.png
    /// </summary>
    Task<string> UploadImageAsync(Guid instructorId, byte[] imageBytes);
}