﻿namespace AmsApi.Models
{
    public class Attendee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public double[]? Embedding { get; set; }
        public int? FaceId { get; set; }

        // Foreign Key to Admin (One-to-Many)
        public int AdminId { get; set; }
        public Admin Admin { get; set; }  // Navigation Property

        // Navigation Property to AttendeeSubject (Many-to-Many)
        public List<AttendeeSubject> AttendeeSubjects { get; set; } = new();
    }
}
