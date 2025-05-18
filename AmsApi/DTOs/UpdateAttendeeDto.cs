namespace AmsApi.DTOs
{

    public class UpdateAttendeeDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public long? Number { get; set; }
        public byte[]? Image { get; set; }
        public string? ImageFileName { get; set; }
        public double[]? Embedding { get; set; }
    }

}
