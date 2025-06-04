using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AmsApi.Models;

public class Subject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreateAt { get; set; }=DateTime.Now;
    public DateTimeOffset UpdatedAt { get; set; }

    // Foreign Key to Instructor (One-to-Many)
    public Guid? InstructorId { get; set; }  // This will store the Instructor's Id
    public Instructor Instructor { get; set; }  // Navigation Property to Instructor
    // Navigation Property to AttendeeSubject (Many-to-Many)
    public List<AttendeeSubject> AttendeeSubjects { get; set; } = new();
    public List<SubjectDate> SubjectDates { get; set; } = new();
}
