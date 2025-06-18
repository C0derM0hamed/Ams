namespace AmsApi.DTOs
{
    public class SubjectSimpleDto
    {

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

