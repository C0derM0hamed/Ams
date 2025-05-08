namespace AmsApi.Models;

public class AttendeeSubject
{
    public int AttendeeId { get; set; }  // Foreign Key to Attendee
    public Attendee Attendee { get; set; }  // Navigation Property to Attendee

    public int SubjectId { get; set; }  // Foreign Key to Subject
    public Subject Subject { get; set; }  // Navigation Property to Subject
}
