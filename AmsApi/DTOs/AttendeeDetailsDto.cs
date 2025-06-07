public class AttendeeDetailsDto
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? ImagePath { get; set; }
    public long Number { get; set; }
    public List<string> SubjectNames { get; set; } = new();
}
