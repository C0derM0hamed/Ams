namespace AmsApi.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<AttendanceDto>> GetAttendanceBySubjectId(int subjectId);
        Task<bool> MarkAttendance(int subjectId, int attendeeId, MarkAttendanceDto dto);
    }
}
