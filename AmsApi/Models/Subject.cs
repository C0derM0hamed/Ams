using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AmsApi.Models;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [NotMapped]
    public List<int> AttendeeIds { get; set; } = new();
    public List<int> InstructorIds { get; set; } = new();
}
