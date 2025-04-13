using AmsApi.Config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace AmsApi.Services;

public class AttendeeService : IAttendeeService
{
    private readonly AmsDbContext _context;
    private readonly JwtSettings _jwt;

    public AttendeeService(AmsDbContext context, IOptions<JwtSettings> jwt)
    {
        _context = context;
        _jwt = jwt.Value;
    }

    public async Task<Attendee> RegisterAsync(RegisterAttendeeDto dto)
    {
        var exists = await _context.Attendees.AnyAsync(a => a.Email == dto.Email);
        if (exists)
            throw new ApiException(400, "Email already registered");

        var newAttendee = new Attendee
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = dto.Password
        };

        _context.Attendees.Add(newAttendee);
        await _context.SaveChangesAsync();

        return newAttendee;
    }

    public async Task<AuthResponse> LoginAsync(LoginAttendeeDto dto)
    {
        var attendee = await _context.Attendees
            .SingleOrDefaultAsync(a => a.Email == dto.Email && a.Password == dto.Password);

        if (attendee == null)
            throw new ApiException(401, "Invalid credentials");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwt.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("attendee_id", attendee.Id.ToString()),
                new Claim(ClaimTypes.Name, attendee.FullName),
                new Claim(ClaimTypes.Email, attendee.Email),
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryInMinutes),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new AuthResponse
        {
            Token = tokenHandler.WriteToken(token),
            ExpiresAt = tokenDescriptor.Expires ?? DateTime.UtcNow
        };
    }

    public async Task<Attendee?> UpdateAsync(int id, UpdateAttendeeDto dto)
    {
        var attendee = await _context.Attendees.FindAsync(id);
        if (attendee == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            attendee.FullName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.Email))
            attendee.Email = dto.Email;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            attendee.Password = dto.Password;

        await _context.SaveChangesAsync();
        return attendee;
    }

    public async Task<Attendee?> GetByIdAsync(int id)
    {
        return await _context.Attendees.FindAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var attendee = await _context.Attendees.FindAsync(id);
        if (attendee == null) return false;

        _context.Attendees.Remove(attendee);
        await _context.SaveChangesAsync();
        return true;
    }
}
