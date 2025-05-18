namespace AmsApi.Models;

public class Attendance
{
    public Guid Id { get; set; }

    public Guid AttendeeId { get; set; }
    public Guid SubjectId { get; set; }
    public Attendee Attendee { get; set; }  
        public Subject  Subject  { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // إضافة CreatedAt


}
