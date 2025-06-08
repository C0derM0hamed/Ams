public class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid AttendeeId { get; set; }
    public string AttendeeName { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
