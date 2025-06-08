namespace AmsApi.DTOs
{
    public class AttendeeReportDto
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public long Number { get; set; }
        public DateTime? LastAttendanceDate { get; set; }
    }
}
