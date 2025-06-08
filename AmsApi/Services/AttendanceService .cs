// AmsApi/Services/AttendanceService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmsApi.Data;
using AmsApi.Interfaces;
using AmsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AmsApi.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AmsDbContext _context;
        private readonly FaceRecognitionService _faceRecognitionService;

        public AttendanceService(AmsDbContext context, FaceRecognitionService faceRecognitionService)
        {
            _context = context;
            _faceRecognitionService = faceRecognitionService;
        }

        public async Task<List<Attendance>> GetBySubjectAsync(Guid subjectId)
        {
            return await _context.Attendances
                .Where(a => a.SubjectId == subjectId)
                .Include(a => a.Attendee)
                .Include(a => a.Subject)
                    .ThenInclude(s => s.Instructor)
                .Include(a => a.Subject)
                    .ThenInclude(s => s.SubjectDates)
                .ToListAsync();
        }

        public async Task<AttendanceDto> CreateOneAsync(Guid subjectId, Guid attendeeId)
        {
            // check if there's already attendance today for this attendee in this subject
            var existsToday = await _context.Attendances
                .AnyAsync(a =>
                    a.AttendeeId == attendeeId &&
                    a.SubjectId == subjectId &&
                    a.CreatedAt.Date == DateTime.UtcNow.Date);

            if (existsToday)
            {
                throw new InvalidOperationException("Duplicate attendance for today");
            }

            var att = new Attendance
            {
                AttendeeId = attendeeId,
                SubjectId = subjectId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Attendances.Add(att);
            await _context.SaveChangesAsync();

            // Load needed data for DTO
            var result = await _context.Attendances
                .Include(a => a.Attendee)
                .Include(a => a.Subject)
                .FirstOrDefaultAsync(a => a.Id == att.Id);

            return new AttendanceDto
            {
                Id = result.Id,
                AttendeeId = result.AttendeeId,
                AttendeeName = result.Attendee.FullName,
                SubjectId = result.SubjectId,
                SubjectName = result.Subject.Name,
                CreatedAt = result.CreatedAt
            };
        }



        public async Task<List<AttendanceDto>> CreateManyAsync(CreateManyAttendanceDto dto)
        {
            var today = DateTime.UtcNow.Date;

            // منع التكرار
            var existing = await _context.Attendances
                .Where(a => a.SubjectId == dto.SubjectId && dto.AttendeeIds.Contains(a.AttendeeId) && a.CreatedAt.Date == today)
                .Select(a => a.AttendeeId)
                .ToListAsync();

            var newAttendances = dto.AttendeeIds
                .Where(id => !existing.Contains(id))
                .Select(id => new Attendance
                {
                    AttendeeId = id,
                    SubjectId = dto.SubjectId,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            _context.Attendances.AddRange(newAttendances);
            await _context.SaveChangesAsync();

            // رجعهم على شكل DTO
            var full = await _context.Attendances
                .Where(a => newAttendances.Select(x => x.Id).Contains(a.Id))
                .Include(a => a.Attendee)
                .Include(a => a.Subject)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    AttendeeId = a.AttendeeId,
                    AttendeeName = a.Attendee.FullName,
                    SubjectId = a.SubjectId,
                    SubjectName = a.Subject.Name,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return full;
        }



        public async Task<bool> DeleteAsync(Guid attendanceId)
        {
            var att = await _context.Attendances.FindAsync(attendanceId);
            if (att == null) return false;
            _context.Attendances.Remove(att);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Attendance> GetByIdAsync(Guid attendanceId)
        {
            var att = await _context.Attendances
                .Include(a => a.Attendee)
                .Include(a => a.Subject)
                    .ThenInclude(s => s.Instructor)
                .Include(a => a.Subject)
                    .ThenInclude(s => s.SubjectDates)
                .FirstOrDefaultAsync(a => a.Id == attendanceId);

            if (att == null) throw new KeyNotFoundException("Attendance not found");
            return att;
        }
        public async Task DeleteAllAsync()
        {
            var all = await _context.Attendances.ToListAsync();
            _context.Attendances.RemoveRange(all);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAllForSubjectAsync(Guid subjectId)
        {
            var attendances = await _context.Attendances
                .Where(a => a.SubjectId == subjectId)
                .ToListAsync();

            _context.Attendances.RemoveRange(attendances);
            await _context.SaveChangesAsync();
        }
        public async Task<AttendanceReportDto> GenerateReportAsync(Guid subjectId)
        {
            var subject = await _context.Subjects
                .Include(s => s.Instructor)
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            if (subject == null) throw new Exception("Subject not found");

            var attendees = await _context.AttendeeSubjects
                .Where(x => x.SubjectId == subjectId)
                .Select(x => x.Attendee)
                .ToListAsync();

            var attendanceData = await _context.Attendances
                .Where(a => a.SubjectId == subjectId)
                .ToListAsync();

            var report = new AttendanceReportDto
            {
                SubjectName = subject.Name,
                InstructorName = subject.Instructor?.FullName ?? "N/A",
                ReportDate = DateTime.UtcNow,
                Attendees = attendees.Select(a => new AttendeeReportDto
                {
                    Id = a.Id,
                    Name = a.FullName,
                    Number = a.Number,
                    LastAttendanceDate = attendanceData
                        .Where(ad => ad.AttendeeId == a.Id)
                        .OrderByDescending(ad => ad.CreatedAt)
                        .Select(ad => ad.CreatedAt)
                        .FirstOrDefault()
                }).ToList()
            };

            return report;
        }
        public async Task<List<CalendarSubjectDateDto>> GetCalendarDatesAsync()
        {
            return await _context.SubjectDates
                .Include(sd => sd.Subject)
                .Select(sd => new CalendarSubjectDateDto
                {
                    SubjectName = sd.Subject.Name,
                    DayOfWeek = sd.DayOfWeek,
                    StartTime = sd.StartTime,
                    EndTime = sd.EndTime
                })
                .ToListAsync();
        }

        public async Task<AttendanceDto> CreateByFaceAsync(IFormFile image, Guid subjectId)
        {
            using var stream = image.OpenReadStream();
            var resultJson = await _faceRecognitionService.ClassifyAsync(stream, image.FileName);

            var result = System.Text.Json.JsonDocument.Parse(resultJson);
            var uuid = result.RootElement.GetProperty("uuid").GetString();

            if (string.IsNullOrWhiteSpace(uuid) || !long.TryParse(uuid, out long studentNumber))
                throw new InvalidOperationException("Invalid face recognition result");

            var attendee = await _context.Attendees.FirstOrDefaultAsync(a => a.Number == studentNumber);
            if (attendee == null)
                throw new KeyNotFoundException($"No attendee found with number {studentNumber}");

            return await CreateOneAsync(subjectId, attendee.Id);
        }



    }
}
