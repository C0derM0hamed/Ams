namespace AmsApi.Models;

public class Admin
{
    public int Id { get; set; } // UUID
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<Attendee> Attendees { get; set; }
}
