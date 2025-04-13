namespace AmsApi.Models;

public class Instructor
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ImagePath { get; set; }

    public List<int> SubjectIds { get; set; } = new();
}

