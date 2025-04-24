namespace AmsApi.DTOs
{
    public class AttendanceReportDto
    {
        public string Subject { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<string> Present { get; set; } = new();
        public List<string> Absent { get; set; } = new();
    }
}
