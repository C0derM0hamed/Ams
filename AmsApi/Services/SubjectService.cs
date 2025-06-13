using AmsApi.DTOs;
using AmsApi.Interfaces;
using AmsApi.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace AmsApi.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly AmsDbContext _context;
        private readonly IHttpContextAccessor _http;
        public SubjectService(AmsDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
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
                 .Include(s => s.SubjectDates)
                 .Where(s => s.Id == id)
                 .Select(s => new SubjectDetailsDto
                 {
                     Id = s.Id,
                     Name = s.Name,
                     Instructor = s.Instructor != null ? s.Instructor.FullName : null,
                     InstructorId = s.InstructorId,
                     CreatedAt = s.CreateAt.Date,
                     SubjectDates = s.SubjectDates.Select(sd => new SubjectDateDto
                     {
                         DayOfWeek = sd.DayOfWeek,
                         StartTime = sd.StartTime,
                         EndTime = sd.EndTime
                     }).ToList()
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

        public async Task<List<AttendeeWithImageDto>> GetAttendeesAsync(Guid subjectId)
        {
            return await _context.AttendeeSubjects
               .Where(asb => asb.SubjectId == subjectId)
               .Select(asb => new AttendeeWithImageDto
               {
                   Id = asb.Attendee.Id,
                   FullName = asb.Attendee.FullName,
                   ImagePath = string.IsNullOrEmpty(asb.Attendee.ImagePath) ? null
                   : $"{_http.HttpContext.Request.Scheme}://{_http.HttpContext.Request.Host}{asb.Attendee.ImagePath}",
                   CreatedAt = asb.Attendee.CreatedAt
               })
               .ToListAsync();
        }

        public async Task<SubjectDate> AddSubjectDateAsync(Guid subjectId, CreateSubjectDateDto dto)
        {
            var subjectDate = new SubjectDate
            {
                SubjectId = subjectId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = TimeSpan.Parse(dto.StartTime),
                EndTime = TimeSpan.Parse(dto.EndTime),
                CreateAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.SubjectDates.Add(subjectDate);
            await _context.SaveChangesAsync();
            return subjectDate;
        }
        public async Task<List<SubjectDateDto>> GetSubjectDatesAsync(Guid subjectId)
        {
            return await _context.SubjectDates
                .Where(sd => sd.SubjectId == subjectId)
                .OrderBy(sd => sd.DayOfWeek)
                .Select(sd => new SubjectDateDto
                {
                    DayOfWeek = sd.DayOfWeek,
                    StartTime = sd.StartTime,
                    EndTime = sd.EndTime
                })
                .ToListAsync();
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
        public async Task<int> DeleteAllAsync()
        {
            var allSubjects = await _context.Subjects.ToListAsync();
            if (!allSubjects.Any())
                return 0;

            _context.Subjects.RemoveRange(allSubjects);
            await _context.SaveChangesAsync();
            return allSubjects.Count;
        }
        public async Task<int> DeleteAllSubjectDatesAsync()
        {
            var allDates = await _context.SubjectDates.ToListAsync();
            if (!allDates.Any())
                return 0;

            _context.SubjectDates.RemoveRange(allDates);
            await _context.SaveChangesAsync();
            return allDates.Count;
        }

    }
}
