﻿namespace AmsApi.Models;

public class Attendance
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public int AttendeeId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public bool IsPresent { get; set; } = false;
    public DateTime? CheckInTime { get; set; } //
}
