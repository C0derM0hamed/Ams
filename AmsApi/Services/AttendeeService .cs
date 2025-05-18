public class AttendeeService : IAttendeeService
{
    private readonly AmsDbContext _context;
    private readonly string _assetsPath;
    private readonly string _secretKey;

    public AttendeeService(AmsDbContext context, string assetsPath, string secretKey)
    {
        _context = context;
        _assetsPath = assetsPath;
        _secretKey = secretKey;
    }

    public async Task<Attendee> CreateAsync(CreateAttendeeDto dto, Guid adminId)
    {
        var attendee = new Attendee
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = dto.Password,
            Number = dto.Number,
            AdminId = adminId

        };

        _context.Attendees.Add(attendee);
        await _context.SaveChangesAsync();
        return attendee;
    }

    public async Task<Attendee> UpdateAsync(Guid id, UpdateAttendeeDto dto)
    {
        var attendee = await _context.Attendees.FindAsync(id);
        if (attendee == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.FullName)) attendee.FullName = dto.FullName;
        if (!string.IsNullOrWhiteSpace(dto.Email)) attendee.Email = dto.Email;
        if (!string.IsNullOrWhiteSpace(dto.Password)) attendee.Password = dto.Password;
        if (dto.Number.HasValue) attendee.Number = dto.Number.Value;

        if (dto.Image != null)
        {
            var imagePath = await SaveImage(attendee, dto.Image);
            attendee.ImagePath = imagePath;
        }

        if (dto.Embedding != null)
        {
            attendee.Embedding = dto.Embedding;
        }

        await _context.SaveChangesAsync();
        return attendee;
    }

    public async Task<Attendee> GetByIdAsync(Guid id)
    {
        return await _context.Attendees.FindAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var attendee = await _context.Attendees.FindAsync(id);
        if (attendee == null) return false;

        _context.Attendees.Remove(attendee);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Attendee>> GetAllAsync()
    {
        return await _context.Attendees.ToListAsync();
    }

    public async Task<Attendee> GetByEmailAsync(string email)
    {
        return await _context.Attendees
                             .FirstOrDefaultAsync(a => a.Email == email);
    }

    private async Task<string> SaveImage(Attendee attendee, byte[] image)
    {
        var attendeeDir = Path.Combine(_assetsPath, attendee.Id.ToString());

        if (!Directory.Exists(attendeeDir))
        {
            Directory.CreateDirectory(attendeeDir);
        }

        var imagePath = Path.Combine(attendeeDir, "profile.png");
        await File.WriteAllBytesAsync(imagePath, image);

        return imagePath;
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

    public async Task<Subject> GetSubjectForAttendee(Guid attendeeId, Guid subjectId)
    {
        var subject = await _context.Subjects
                                    .FirstOrDefaultAsync(s => s.Id == subjectId &&
                                                               s.AttendeeSubjects.Any(a => a.AttendeeId == attendeeId));
        return subject;
    }
    public async Task UploadImageAsync(Guid attendeeId, byte[] imageBytes)
    {
        var attendee = await _context.Attendees.FindAsync(attendeeId);
        if (attendee == null)
        {
            throw new Exception("Attendee not found");
        }

        // حفظ الصورة في المسار المناسب أو في قاعدة البيانات
        // في هذه الحالة، نقوم بتخزين مسار الصورة فقط
        attendee.ImagePath = "path_to_image";  // هنا يمكنك تخزين الصورة أو مسارها
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
    // تنفيذ الدالة لاسترجاع الموضوعات الخاصة بالمتدرب
    public async Task<List<Subject>> GetSubjectsForAttendeeAsync(Guid attendeeId)
    {
        var attendee = await _context.Attendees
            .Include(a => a.AttendeeSubjects) // ربط الموضوعات بالمتدرب
            .ThenInclude(s => s.Subject) // تضمين الـ Subject الخاص بكل موضوع
            .FirstOrDefaultAsync(a => a.Id == attendeeId);

        if (attendee == null)
        {
            throw new Exception("Attendee not found");
        }

        return attendee.AttendeeSubjects.Select(s => s.Subject).ToList(); // إرجاع الموضوعات
    }


}
