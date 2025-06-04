namespace AmsApi.Models
{
    public class Attendee
    {
        public Guid Id { get; set; }
        public long Number { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public double[]? Embedding { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;



        // Navigation Property to AttendeeSubject (Many-to-Many)
        public List<AttendeeSubject> AttendeeSubjects { get; set; } = new();
    }
}
