namespace AmsApi.DTOs
{
    public class SubjectDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public Guid? InstructorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SubjectDateDto> SubjectDates { get; set; } = new();
    }
}
