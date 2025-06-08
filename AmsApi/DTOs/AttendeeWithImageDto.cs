public class AttendeeWithImageDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
}
