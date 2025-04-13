namespace AmsApi.DTOs
{
    public class AttendeeDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // ده اللي بيتم توليده أوتوماتيك باستخدام الـ Resolver
        public string? ImageUrl { get; set; }


    }

}
