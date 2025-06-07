using AmsApi.Models;
using AmsApi.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using AmsApi.Config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace AmsApi.Services;

public class InstructorService : IInstructorService
{
    private readonly AmsDbContext _context;
    private readonly string _assetsRoot;
    private readonly IHttpContextAccessor _http;

    public InstructorService(AmsDbContext context, IWebHostEnvironment env, IHttpContextAccessor http)
    {
        _context = context;
        _http = http;
        _assetsRoot = Path.Combine(env.WebRootPath, "instructors");
    }

    public async Task<List<InstructorListDto>> GetAllAsync()
    {
        return await _context.Instructors
            .Select(i => new InstructorListDto
            {
                Id = i.Id,
                FullName = i.FullName,
                 ImagePath = string.IsNullOrEmpty(i.ImagePath)
                ? null
                : $"{_http.HttpContext.Request.Scheme}://{_http.HttpContext.Request.Host}{i.ImagePath}"
            })
            .ToListAsync();
    }

    public async Task<InstructorDetailsDto?> GetByIdAsync(Guid id)
    {
        var instructor = await _context.Instructors
            .Include(i => i.Subjects)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instructor == null) return null;

        return new InstructorDetailsDto
        {
            FullName = instructor.FullName,
            Email = instructor.Email,
            Password = instructor.Password,
            Number = instructor.Number,
            ImagePath = string.IsNullOrEmpty(instructor.ImagePath)
                ? null
                : $"{_http.HttpContext.Request.Scheme}://{_http.HttpContext.Request.Host}{instructor.ImagePath}",
            SubjectNames = instructor.Subjects.Select(s => s.Name).ToList()
        };
    }

    public async Task<Instructor?> GetByEmailAsync(string email)
        => await _context.Instructors.FirstOrDefaultAsync(x => x.Email == email);

    public async Task<Instructor> CreateAsync(CreateInstructorDto dto)
    {
        var instructor = new Instructor
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = dto.Password,
            Number = dto.Number,
        };

        _context.Instructors.Add(instructor);
        await _context.SaveChangesAsync();
        return instructor;
    }

    public async Task<Instructor?> UpdateAsync(Guid id, UpdateInstructorDto dto)
    {
        var inst = await _context.Instructors.FindAsync(id);
        if (inst == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            inst.FullName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.Email))
            inst.Email = dto.Email;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            inst.Password = dto.Password;

        if (dto.Number.HasValue)
            inst.Number = dto.Number.Value;

        await _context.SaveChangesAsync();
        return inst;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var inst = await _context.Instructors.FindAsync(id);
        if (inst == null) return false;
        _context.Instructors.Remove(inst);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SubjectSimpleDto>> GetSubjectsForInstructorAsync(Guid instructorId)
    {
        return await _context.Subjects
            .Where(s => s.InstructorId == instructorId)
            .Select(s => new SubjectSimpleDto
            {
                Name = s.Name,
                CreatedAt = s.CreateAt.UtcDateTime
            })
            .ToListAsync();
    }

    public async Task<SubjectSimpleDto?> GetSubjectForInstructorAsync(Guid instructorId, Guid subjectId)
    {
        return await _context.Subjects
            .Where(s => s.Id == subjectId && s.InstructorId == instructorId)
            .Select(s => new SubjectSimpleDto
            {
                Name = s.Name,
                CreatedAt = s.CreateAt.UtcDateTime
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> AssignSubjectToInstructorAsync(Guid instructorId, Guid subjectId)
    {
        var subject = await _context.Subjects.FindAsync(subjectId);
        if (subject == null) return false;
        subject.InstructorId = instructorId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveSubjectFromInstructorAsync(Guid instructorId, Guid subjectId)
    {
        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == subjectId && s.InstructorId == instructorId);
        if (subject == null) return false;
        subject.InstructorId = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> UploadImageAsync(Guid instructorId, byte[] imageBytes)
    {
        var relativePath = Path.Combine("instructors", instructorId.ToString());
        var dir = Path.Combine("wwwroot", relativePath);

        Directory.CreateDirectory(dir);

        var fileName = "Instructor.png";
        var fullPath = Path.Combine(dir, fileName);

        await System.IO.File.WriteAllBytesAsync(fullPath, imageBytes);

        var inst = await _context.Instructors.FindAsync(instructorId);
        if (inst == null) throw new InvalidOperationException("Instructor not found");

        // ✅ احفظ المسار النسبي
        inst.ImagePath = $"/{relativePath.Replace("\\", "/")}/{fileName}";
        await _context.SaveChangesAsync();

        return inst.ImagePath;
    }


    private async Task<string> SaveImage(Guid InstrutorId, byte[] image)
    {
        var attendeeDir = Path.Combine("wwwroot", "Instructor", InstrutorId.ToString());

        if (!Directory.Exists(attendeeDir))
            Directory.CreateDirectory(attendeeDir);

        var fileName = "Instructor.png";
        var fullPath = Path.Combine(attendeeDir, fileName);

        await System.IO.File.WriteAllBytesAsync(fullPath, image);

        return $"/Instructor/{InstrutorId}/{fileName}";
    }
}
