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

        public AttendanceService(AmsDbContext context)
        {
            _context = context;
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

        public async Task<Attendance> CreateOneAsync(Guid subjectId, Guid attendeeId)
        {
            // prevent duplicate in same day
            var last = await _context.Attendances
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (last != null
                && last.AttendeeId == attendeeId
                && last.CreatedAt.Date == DateTime.UtcNow.Date)
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

            return await GetByIdAsync(att.Id);
        }

        public async Task<List<Attendance>> CreateManyAsync(Guid subjectId, List<Guid> attendeeIds)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            var created = new List<Attendance>();
            foreach (var attendeeId in attendeeIds)
            {
                var a = await CreateOneAsync(subjectId, attendeeId);
                created.Add(a);
            }
            await tx.CommitAsync();
            return created;
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
    }
}
