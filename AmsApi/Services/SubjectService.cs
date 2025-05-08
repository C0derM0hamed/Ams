using AmsApi.Data;
using AmsApi.DTOs;
using AmsApi.Interfaces;
using AmsApi.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AmsApi.Services;

public class SubjectService : ISubjectService
{
    private readonly AmsDbContext _context;
    private readonly IMapper _mapper;

    public SubjectService(AmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SubjectDto>> GetAllAsync()
    {
        var subjects = await _context.Subjects.ToListAsync();
        return _mapper.Map<IEnumerable<SubjectDto>>(subjects);
    }

    public async Task<SubjectDto?> GetByIdAsync(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        return subject is null ? null : _mapper.Map<SubjectDto>(subject);
    }

    public async Task<SubjectDto> CreateAsync(CreateSubjectDto dto)
    {
        var subject = _mapper.Map<Subject>(dto);
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        return _mapper.Map<SubjectDto>(subject);
    }

    public async Task<bool> UpdateAsync(int id, UpdateSubjectDto dto)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null) return false;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            subject.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Description))
            subject.Description = dto.Description;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null) return false;

        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();
        return true;
    }

    // تعديل إضافة Attendee إلى Subject باستخدام AttendeeSubject
    public async Task<bool> AddAttendeeToSubject(int subjectId, int attendeeId)
    {
        var subject = await _context.Subjects.FindAsync(subjectId);
        if (subject == null) return false;

        // تحقق من إذا كان الـ AttendeeSubject موجودًا بالفعل
        var existingAttendeeSubject = await _context.AttendeeSubjects
            .FirstOrDefaultAsync(sd => sd.SubjectId == subjectId && sd.AttendeeId == attendeeId);

        if (existingAttendeeSubject == null)
        {
            var attendeeSubject = new AttendeeSubject
            {
                AttendeeId = attendeeId,
                SubjectId = subjectId
            };

            _context.AttendeeSubjects.Add(attendeeSubject);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    // تعديل إزالة Attendee من Subject باستخدام AttendeeSubject
    public async Task<bool> RemoveAttendeeFromSubject(int subjectId, int attendeeId)
    {
        var subject = await _context.Subjects.FindAsync(subjectId);
        if (subject == null) return false;

        var attendeeSubject = await _context.AttendeeSubjects
            .FirstOrDefaultAsync(sd => sd.SubjectId == subjectId && sd.AttendeeId == attendeeId);

        if (attendeeSubject != null)
        {
            _context.AttendeeSubjects.Remove(attendeeSubject);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    // تعديل إضافة Instructor إلى Subject باستخدام AttendeeSubject
    public async Task<bool> AddInstructorToSubject(int subjectId, int instructorId)
    {
        var subject = await _context.Subjects.FindAsync(subjectId);
        if (subject == null) return false;

        // تحقق من أن الـ InstructorId ليس موجودًا بالفعل
        if (subject.InstructorId != instructorId)
        {
            subject.InstructorId = instructorId;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    // تعديل إزالة Instructor من Subject
    public async Task<bool> RemoveInstructorFromSubject(int subjectId, int instructorId)
    {
        var subject = await _context.Subjects.FindAsync(subjectId);
        if (subject == null) return false;

        // تحقق من أن الـ InstructorId موجود في المادة
        if (subject.InstructorId == instructorId)
        {
            subject.InstructorId = 0; // أو ضع الـ InstructorId كـ null إذا كنت تستخدم nullables
            await _context.SaveChangesAsync();
        }

        return true;
    }
}
