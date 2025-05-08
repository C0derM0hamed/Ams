using AmsApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace AmsApi.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AmsDbContext _context;
    private readonly IMapper _mapper;

    public AttendanceService(AmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AttendanceDto>> GetAttendanceBySubjectId(int subjectId)
    {
        var records = await _context.Attendances
            .Where(a => a.SubjectId == subjectId)
            .ToListAsync();

        return _mapper.Map<List<AttendanceDto>>(records);
    }

    public async Task<bool> MarkAttendance(int subjectId, int attendeeId, MarkAttendanceDto dto)
    {
        var today = DateTime.UtcNow.Date;

        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a =>
                a.SubjectId == subjectId &&
                a.AttendeeId == attendeeId &&
                a.Date.Date == today);

        if (existing != null)
        {
            existing.IsPresent = dto.IsPresent;
        }
        else
        {
            var newAttendance = new Attendance
            {
                SubjectId = subjectId,
                AttendeeId = attendeeId,
                Date = today,
                IsPresent = dto.IsPresent
            };

            _context.Attendances.Add(newAttendance);
        }

        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<AttendanceReportDto?> GetAttendanceReportAsync(int subjectId, DateTime date)
    {
        var subject = await _context.Subjects.FindAsync(subjectId);
        if (subject == null)
            return null;

        // جلب الـ Attendees المرتبطين بالـ Subject عبر AttendeeSubjects
        var subjectAttendees = await _context.Attendees
        .Where(a => a.AttendeeSubjects.Any(ad => ad.SubjectId == subjectId)) // استخدام AttendeeSubjects بدلاً من SubjectIds
            .ToListAsync();

        // جلب الحضور بناءً على الـ SubjectId و التاريخ
        var presentIds = await _context.Attendances
            .Where(a => a.SubjectId == subjectId && a.Date.Date == date.Date)
            .Select(a => a.AttendeeId)
            .ToListAsync();

        // تحديد الحضور
        var present = subjectAttendees
            .Where(a => presentIds.Contains(a.Id))
            .Select(a => a.FullName)
            .ToList();

        // تحديد الغياب
        var absent = subjectAttendees
            .Where(a => !presentIds.Contains(a.Id))
            .Select(a => a.FullName)
            .ToList();

        return new AttendanceReportDto
        {
            Subject = subject.Name,
            Date = date,
            Present = present,
            Absent = absent
        };
    }


    public byte[] GenerateAttendancePdf(string subjectName, DateTime date, List<string> present, List<string> absent)
    {
        using var ms = new MemoryStream();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Content().Column(col =>
                {
                    col.Item().Text($"تقرير الحضور").FontSize(18).Bold().AlignCenter();
                    col.Item().Text($"المادة: {subjectName}").FontSize(14).AlignRight();
                    col.Item().Text($"التاريخ: {date:yyyy-MM-dd}").FontSize(14).AlignRight();

                    col.Item().PaddingVertical(10);

                    col.Item().Text("✅ الحاضرين").FontSize(16).Bold();
                    col.Item().Text(string.Join("\n", present)).FontSize(14);

                    col.Item().PaddingVertical(10);

                    col.Item().Text("❌ الغائبين").FontSize(16).Bold();
                    col.Item().Text(string.Join("\n", absent)).FontSize(14);
                });
            });
        })
        .GeneratePdf(ms);

        return ms.ToArray();
    }



}
