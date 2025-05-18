namespace AmsApi.DTOs
{
    public class UpdateSubjectDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? InstructorId { get; set; }
    }

}
