﻿public class CreateUserDto
{
    public Guid Id { get; set; } // إضافة الـ Id هنا
    public string FullName { get; set; }
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!; // Admin / Instructor / Attendee
}
