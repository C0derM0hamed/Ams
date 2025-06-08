using Microsoft.IdentityModel.Tokens;
using static System.Net.WebRequestMethods;

public class AttendeeService : IAttendeeService
{
    private readonly AmsDbContext _context;
    private readonly IHttpContextAccessor _http;

    public AttendeeService(AmsDbContext context,IHttpContextAccessor http)
    {
        _context = context;
        _http = http;
    }

    public async Task<Attendee> CreateAsync(CreateAttendeeDto dto, Guid adminId)
    {
        var attendee = new Attendee
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = dto.Password,
            Number = dto.Number,   
        };

        _context.Attendees.Add(attendee);
        await _context.SaveChangesAsync();
        return attendee;
    }

    public async Task<UpdateAttendeeDto?> UpdateAsync(Guid id, UpdateAttendeeDto dto)
{
    var attendee = await _context.Attendees.FindAsync(id);
    if (attendee == null) return null;

    if (!string.IsNullOrWhiteSpace(dto.FullName))
        attendee.FullName = dto.FullName;

    if (!string.IsNullOrWhiteSpace(dto.Email))
        attendee.Email = dto.Email;

    if (!string.IsNullOrWhiteSpace(dto.Password))
        attendee.Password = dto.Password;

    if (dto.Number.HasValue)
        attendee.Number = dto.Number.Value;
    await _context.SaveChangesAsync();

    return new UpdateAttendeeDto
    {
        FullName = attendee.FullName,
        Email = attendee.Email,
        Password = attendee.Password,
        Number = attendee.Number,
    };
}

    public async Task<AttendeeDetailsDto?> GetByIdAsync(Guid id)
    {
        var attendee = await _context.Attendees
            .Include(a => a.AttendeeSubjects)
            .ThenInclude(asb => asb.Subject)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attendee == null) return null;

        return new AttendeeDetailsDto
        {
            FullName = attendee.FullName,
            Email = attendee.Email,
            Number=attendee.Number,
            Password = attendee.Password,
            ImagePath = string.IsNullOrEmpty(attendee.ImagePath)
                ? null
                : $"{_http.HttpContext.Request.Scheme}://{_http.HttpContext.Request.Host}/{attendee.ImagePath}",
            SubjectNames = attendee.AttendeeSubjects.Select(s => s.Subject.Name).ToList()
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var attendee = await _context.Attendees.FindAsync(id);
        if (attendee == null) return false;

        _context.Attendees.Remove(attendee);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AttendeeSummaryDto>> GetAllAsync()
    {
        var attendees = await _context.Attendees.ToListAsync();

        var result = attendees.Select(a => new AttendeeSummaryDto
        {
            Id = a.Id,
            FullName = a.FullName,
            ImagePath = string.IsNullOrEmpty(a.ImagePath)
                ? null
                : $"{_http.HttpContext.Request.Scheme}://{_http.HttpContext.Request.Host}/{a.ImagePath}"
        }).ToList();

        return result;
    }

    public async Task<Attendee> GetByEmailAsync(string email)
    {
        return await _context.Attendees
                             .FirstOrDefaultAsync(a => a.Email == email);
    }

    private async Task<string> SaveImage(Guid attendeeId, byte[] image)
    {
        var attendeeDir = Path.Combine("wwwroot", "uploads", attendeeId.ToString());

        if (!Directory.Exists(attendeeDir))
            Directory.CreateDirectory(attendeeDir);

        var fileName = "profile.png";
        var fullPath = Path.Combine(attendeeDir, fileName);

        await System.IO.File.WriteAllBytesAsync(fullPath, image);

        //  يرجع المسار النسبي للعرض في الفرونت
        return $"/uploads/{attendeeId}/profile.png";
    }
    public async Task<bool> AddSubjectToAttendee(Guid attendeeId, Guid subjectId)
    {
        var attendee = await _context.Attendees.FindAsync(attendeeId);
        var subject = await _context.Subjects.FindAsync(subjectId);

        if (attendee == null || subject == null)
        {
            return false;
        }

        // إضافة العلاقة بين Attendee و Subject في جدول "AttendeeSubjects"
        var attendeeSubject = new AttendeeSubject
        {
            AttendeeId = attendeeId,
            SubjectId = subjectId
        };

        await _context.AttendeeSubjects.AddAsync(attendeeSubject);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<SubjectSimpleDto?> GetSubjectForAttendee(Guid attendeeId, Guid subjectId)
    {
        return await _context.AttendeeSubjects
            .Where(x => x.AttendeeId == attendeeId && x.SubjectId == subjectId)
            .Select(x => new SubjectSimpleDto
            {
                Name = x.Subject.Name,
                CreatedAt = x.Subject.CreateAt.UtcDateTime
            })
            .FirstOrDefaultAsync();
    }
    public async Task<List<SubjectSimpleDto>> GetSubjectsForAttendeeAsync(Guid attendeeId)
    {
        return await _context.AttendeeSubjects
            .Where(x => x.AttendeeId == attendeeId)
            .Select(x => new SubjectSimpleDto
            {
                Name = x.Subject.Name,
                CreatedAt = x.Subject.CreateAt.UtcDateTime
            })
            .ToListAsync();
    }

    public async Task UploadImageAsync(Guid attendeeId, byte[] imageBytes)
    {
        var attendee = await _context.Attendees.FindAsync(attendeeId);
        if (attendee == null)
            throw new Exception("Attendee not found");

        var imagePath = await SaveImage(attendee.Id, imageBytes);
        attendee.ImagePath = imagePath;

        await _context.SaveChangesAsync();
    }
    // تنفيذ إزالة الموضوع من المتدرب
    public async Task<bool> RemoveSubjectFromAttendee(Guid attendee_id, Guid subject_id)
    {
        var attendee = await _context.Attendees
            .Include(a => a.AttendeeSubjects)
            .FirstOrDefaultAsync(a => a.Id == attendee_id);

        if (attendee == null)
        {
            return false; // المتدرب غير موجود
        }

        var subjectToRemove = attendee.AttendeeSubjects
            .FirstOrDefault(a => a.SubjectId == subject_id);

        if (subjectToRemove == null)
        {
            return false; // الموضوع غير موجود
        }

        // إزالة الموضوع من المتدرب
        _context.AttendeeSubjects.Remove(subjectToRemove);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> DeleteAllAsync()
    {
        var allAttendees = await _context.Attendees.ToListAsync();

        if (!allAttendees.Any())
            return 0;

        _context.Attendees.RemoveRange(allAttendees);
        await _context.SaveChangesAsync();

        return allAttendees.Count;
    }
}
