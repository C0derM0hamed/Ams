using System.ComponentModel.DataAnnotations.Schema;

namespace AmsApi.Models;

public class SubjectDate
{
    public Guid Id { get; set; }

    public int DayOfWeek { get; set; }

    [Column(TypeName = "time")]
    public TimeSpan StartTime { get; set; }

    [Column(TypeName = "time")]
    public TimeSpan EndTime { get; set; }

    public Guid SubjectId { get; set; }

    [ForeignKey(nameof(SubjectId))]
    public Subject Subject { get; set; }

    public DateTimeOffset CreateAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

}

