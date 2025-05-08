using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AmsApi.Models;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Foreign Key to Instructor (One-to-Many)
    public int InstructorId { get; set; }  // This will store the Instructor's Id
    public Instructor Instructor { get; set; }  // Navigation Property to Instructor
    // Navigation Property to AttendeeSubject (Many-to-Many)
    public List<AttendeeSubject> AttendeeSubjects { get; set; } = new();

}
