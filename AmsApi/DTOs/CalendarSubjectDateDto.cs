namespace AmsApi.DTOs
{
    public class CalendarSubjectDateDto
    {
        public string SubjectName { get; set; }
        public int DayOfWeek { get; set; } // 0 = Sunday, 6 = Saturday
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
