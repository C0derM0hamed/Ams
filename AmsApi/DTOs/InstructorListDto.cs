namespace AmsApi.DTOs
{
    public class InstructorListDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
    }
}
