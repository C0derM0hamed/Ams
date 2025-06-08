namespace AmsApi.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<Attendance>> GetBySubjectAsync(Guid subjectId);
        Task<AttendanceDto> CreateOneAsync(Guid subjectId, Guid attendeeId);
        Task<List<AttendanceDto>> CreateManyAsync(CreateManyAttendanceDto dto);
        Task<bool> DeleteAsync(Guid attendanceId);
        Task<Attendance> GetByIdAsync(Guid attendanceId);
        Task DeleteAllAsync();
        Task DeleteAllForSubjectAsync(Guid subjectId);
        Task<AttendanceReportDto> GenerateReportAsync(Guid subjectId);
        Task<List<CalendarSubjectDateDto>> GetCalendarDatesAsync();
        Task<AttendanceDto> CreateByFaceAsync(IFormFile image, Guid subjectId);

    }

}

