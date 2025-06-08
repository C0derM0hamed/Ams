namespace AmsApi.DTOs
{
    public class AttendanceReportDto
    {
        public string SubjectName { get; set; }
        public string InstructorName { get; set; }
        public DateTime ReportDate { get; set; }
        public List<AttendeeReportDto> Attendees { get; set; } = new();
    }
}
