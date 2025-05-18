namespace AmsApi.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<Attendance>> GetBySubjectAsync(Guid subjectId);
        Task<Attendance> CreateOneAsync(Guid subjectId, Guid attendeeId);
        Task<List<Attendance>> CreateManyAsync(Guid subjectId, List<Guid> attendeeIds);
        Task<bool> DeleteAsync(Guid attendanceId);
        Task<Attendance> GetByIdAsync(Guid attendanceId);
    }

}

