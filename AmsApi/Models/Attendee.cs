namespace AmsApi.Models;

public class Attendee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ImagePath { get; set; } // ممكن يكون null
    public List<int> SubjectIds { get; set; } = new();
}
