using AmsApi.Config;
using AmsApi.Data;
using AmsApi.DTOs;
using AmsApi.Errors;
using AmsApi.Interfaces;
using AmsApi.Models;
using AmsApi.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AmsApi.Services;

public class InstructorService : IInstructorService
{
    private readonly AmsDbContext _context;
    private readonly JwtSettings _jwt;

    public InstructorService(AmsDbContext context, IOptions<JwtSettings> jwt)
    {
        _context = context;
        _jwt = jwt.Value;
    }

    public async Task<Instructor> RegisterAsync(RegisterInstructorDto dto)
    {
        var exists = await _context.Instructors.AnyAsync(x => x.Email == dto.Email);
        if (exists)
            throw new ApiException(400, "Email already registered");

        var newInstructor = new Instructor
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = dto.Password
        };

        _context.Instructors.Add(newInstructor);
        await _context.SaveChangesAsync();

        return newInstructor;
    }

    public async Task<AuthResponse> LoginAsync(LoginInstructorDto dto)
    {
        var instructor = await _context.Instructors
            .SingleOrDefaultAsync(x => x.Email == dto.Email && x.Password == dto.Password);

        if (instructor == null)
            throw new ApiException(401, "Invalid credentials");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwt.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("instructor_id", instructor.Id.ToString()),
                new Claim(ClaimTypes.Name, instructor.FullName),
                new Claim(ClaimTypes.Email, instructor.Email),
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryInMinutes),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new AuthResponse
        {
            Token = tokenHandler.WriteToken(token),
            ExpiresAt = tokenDescriptor.Expires ?? DateTime.UtcNow
        };
    }

    public async Task<Instructor?> UpdateAsync(int id, UpdateInstructorDto dto)
    {
        var instructor = await _context.Instructors.FindAsync(id);
        if (instructor == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            instructor.FullName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.Email))
            instructor.Email = dto.Email;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            instructor.Password = dto.Password;

        await _context.SaveChangesAsync();
        return instructor;
    }

    public async Task<Instructor?> GetByIdAsync(int id)
    {
        return await _context.Instructors.FindAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var instructor = await _context.Instructors.FindAsync(id);
        if (instructor == null) return false;

        _context.Instructors.Remove(instructor);
        await _context.SaveChangesAsync();
        return true;
    }
}
