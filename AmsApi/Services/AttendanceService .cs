

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

}
