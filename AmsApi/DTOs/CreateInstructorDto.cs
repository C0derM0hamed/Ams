namespace AmsApi.DTOs
{
    public class CreateInstructorDto
    {
        public string FullName { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
