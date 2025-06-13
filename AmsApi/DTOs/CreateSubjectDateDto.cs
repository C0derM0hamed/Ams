using System.ComponentModel.DataAnnotations;

public class CreateSubjectDateDto
{
    public int DayOfWeek { get; set; }

    [RegularExpression(@"^([01]?\d|2[0-3]):[0-5]\d(:[0-5]\d)?$", ErrorMessage = "StartTime must be a valid time in HH:mm or HH:mm:ss format")]
    public string StartTime { get; set; }

    [RegularExpression(@"^([01]?\d|2[0-3]):[0-5]\d(:[0-5]\d)?$", ErrorMessage = "EndTime must be a valid time in HH:mm or HH:mm:ss format")]
    public string EndTime { get; set; }
}