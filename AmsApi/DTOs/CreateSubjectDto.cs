namespace AmsApi.DTOs
{
    public class CreateSubjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? InstructorId { get; set; }
    }
}
