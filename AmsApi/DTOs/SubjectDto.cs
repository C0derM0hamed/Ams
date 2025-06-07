namespace AmsApi.DTOs
{
    public class SubjectDto
    {
        public Guid Id { get; set; }  // تغيير الـ Id إلى Guid
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? InstructorId { get; set; }
    }
}

