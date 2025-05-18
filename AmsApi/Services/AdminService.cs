using AmsApi.DTOs;
using AmsApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AmsApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly AmsDbContext _context;
        private readonly string _secretKey = "your_secret_key"; // Secret key for JWT

        public AdminService(AmsDbContext context)
        {
            _context = context;
        }

        // LoginAsync method for Admin login
        public async Task<AuthResponse> LoginAsync(LoginDto dto)
        {
            var admin = await _context.Admins.SingleOrDefaultAsync(a => a.Email == dto.Username);
            if (admin == null || admin.Password != dto.Password)
                throw new UnauthorizedAccessException("Invalid credentials");

            var token = JwtHelper.GenerateToken(admin.Id, "Admin");

            return new AuthResponse { Token = token };
        }

    }
}
