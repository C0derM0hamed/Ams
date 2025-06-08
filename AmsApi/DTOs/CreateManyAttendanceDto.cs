public class CreateManyAttendanceDto
{
    public List<Guid> AttendeeIds { get; set; } = new();
    public Guid SubjectId { get; set; }
}
