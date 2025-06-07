using AmsApi.DTOs;
using AmsApi.Interfaces;
using AmsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AmsApi.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly AmsDbContext _context;

        public SubjectService(AmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubjectListDto>> GetAllAsync()
        {
            return await _context.Subjects
                .Include(s => s.Instructor)
                .Select(s => new SubjectListDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();
        }

        public async Task<SubjectDetailsDto?> GetByIdAsync(Guid id)
        {
            return await _context.Subjects
                .Include(s => s.Instructor)
                .Where(s => s.Id == id)
                .Select(s => new SubjectDetailsDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Instructor = s.Instructor != null ? s.Instructor.FullName : null,
                    InstructorId = s.InstructorId,
                    CreatedAt = s.CreateAt.Date
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Subject> CreateAsync(CreateSubjectDto dto)
        {
            var subject = new Subject
            {
                Name = dto.Name,
            };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
            return subject;
        }

        public async Task<Subject?> UpdateAsync(Guid id, UpdateSubjectDto dto)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                subject.Name = dto.Name;
            if (dto.InstructorId.HasValue)
                subject.InstructorId = dto.InstructorId;

            await _context.SaveChangesAsync();
            return subject;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return false;

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Attendee>> GetAttendeesAsync(Guid subjectId)
        {
            var attendees = await _context.AttendeeSubjects
                .Where(asb => asb.SubjectId == subjectId)
                .Select(asb => asb.Attendee)
                .ToListAsync();

            return attendees;
        }

        public async Task<SubjectDate> AddSubjectDateAsync(Guid subjectId, CreateSubjectDateDto dto)
        {
            var subjectDate = new SubjectDate
            {
                SubjectId = subjectId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                CreateAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.SubjectDates.Add(subjectDate);
            await _context.SaveChangesAsync();
            return subjectDate;
        }

        public async Task<bool> RemoveSubjectDateAsync(Guid subjectId, Guid subjectDateId)
        {
            var subjectDate = await _context.SubjectDates
                .FirstOrDefaultAsync(sd => sd.SubjectId == subjectId && sd.Id == subjectDateId);

            if (subjectDate == null) return false;

            _context.SubjectDates.Remove(subjectDate);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
