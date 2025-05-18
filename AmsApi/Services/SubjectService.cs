namespace AmsApi.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly AmsDbContext _context;
        public SubjectService(AmsDbContext context) => _context = context;

        public async Task<List<Subject>> GetAllAsync()
            => await _context.Subjects
                .Include(s => s.Instructor)
                .Include(s => s.AttendeeSubjects).ThenInclude(x => x.Attendee)
                .ToListAsync();

        public async Task<Subject?> GetByIdAsync(Guid id)
            => await _context.Subjects
                .Include(s => s.Instructor)
                .Include(s => s.AttendeeSubjects).ThenInclude(x => x.Attendee)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<Subject> CreateAsync(CreateSubjectDto dto)
        {
            var s = new Subject
            {
                Name = dto.Name,
                Description = dto.Description,
                InstructorId = dto.InstructorId
            };
            _context.Subjects.Add(s);
            await _context.SaveChangesAsync();
            return s;
        }

        public async Task<Subject?> UpdateAsync(Guid id, UpdateSubjectDto dto)
        {
            var s = await _context.Subjects.FindAsync(id);
            if (s == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                s.Name = dto.Name;

            if (dto.Description != null)
                s.Description = dto.Description;

            if (dto.InstructorId.HasValue)
                s.InstructorId = dto.InstructorId; // can be null to unassign

            await _context.SaveChangesAsync();
            return s;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var s = await _context.Subjects.FindAsync(id);
            if (s == null) return false;
            _context.Subjects.Remove(s);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Attendee>> GetAttendeesAsync(Guid subjectId)
        {
            return await _context.AttendeeSubjects
                .Where(x => x.SubjectId == subjectId)
                .Select(x => x.Attendee)
                .ToListAsync();
        }

        public async Task<SubjectDate> AddSubjectDateAsync(Guid subjectId, CreateSubjectDateDto dto)
        {
            var sd = new SubjectDate
            {
                SubjectId = subjectId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };
            _context.SubjectDates.Add(sd);
            await _context.SaveChangesAsync();
            return sd;
        }

        public async Task<bool> RemoveSubjectDateAsync(Guid subjectId, Guid subjectDateId)
        {
            var sd = await _context.SubjectDates
                .FirstOrDefaultAsync(x => x.Id == subjectDateId && x.SubjectId == subjectId);
            if (sd == null) return false;
            _context.SubjectDates.Remove(sd);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
