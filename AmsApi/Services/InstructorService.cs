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

namespace AmsApi.Services;

public class InstructorService : IInstructorService
{
    private readonly AmsDbContext _context;
    private readonly string _assetsRoot;

    public InstructorService(AmsDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        // We'll save images under wwwroot/instructors/{id}/image.png
        _assetsRoot = Path.Combine(env.WebRootPath, "instructors");
    }

    public async Task<List<Instructor>> GetAllAsync()
        => await _context.Instructors.ToListAsync();

    public async Task<Instructor?> GetByIdAsync(Guid id)
        => await _context.Instructors.FindAsync(id);

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

        if (dto.ImageBytes != null)
        {
            var path = await SaveImageAsync(id, dto.ImageBytes);
            inst.ImagePath = path;
        }

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

    public async Task<List<Subject>> GetSubjectsForInstructorAsync(Guid instructorId)
    {
        return await _context.Subjects
            .Where(s => s.InstructorId == instructorId)
            .ToListAsync();
    }

    public async Task<Subject?> GetSubjectForInstructorAsync(Guid instructorId, Guid subjectId)
    {
        return await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == subjectId && s.InstructorId == instructorId);
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
        var dir = Path.Combine(_assetsRoot, instructorId.ToString());
        Directory.CreateDirectory(dir);
        var filePath = Path.Combine(dir, "image.png");
        await File.WriteAllBytesAsync(filePath, imageBytes);

        // Update DB record
        var inst = await _context.Instructors.FindAsync(instructorId);
        if (inst == null) throw new InvalidOperationException("Instructor not found");
        inst.ImagePath = filePath;
        await _context.SaveChangesAsync();
        return filePath;
    }

    private Task<string> SaveImageAsync(Guid instructorId, byte[] bytes)
        => UploadImageAsync(instructorId, bytes);
}

