namespace AmsApi.Interfaces
{
    public interface ISubjectDateService
    {
        Task<List<SubjectDate>> GetDatesBySubjectId(int subjectId);
        Task<SubjectDate> AddAsync(CreateSubjectDateDto dto);
    }
}
