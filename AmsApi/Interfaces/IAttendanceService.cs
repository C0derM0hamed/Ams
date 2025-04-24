namespace AmsApi.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<AttendanceDto>> GetAttendanceBySubjectId(int subjectId);
        Task<bool> MarkAttendance(int subjectId, int attendeeId, MarkAttendanceDto dto);
        Task<AttendanceReportDto?> GetAttendanceReportAsync(int subjectId, DateTime date);
        byte[] GenerateAttendancePdf(string subjectName, DateTime date, List<string> present, List<string> absent);
    }
}
