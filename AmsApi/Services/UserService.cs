using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace AmsApi.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtHelper _jwtHelper;

        public UserService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IJwtHelper jwtHelper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtHelper = jwtHelper;
        }

        public async Task<RegisterResponse> RegisterUserAsync(CreateUserDto dto)
        {
            // تأكد إن الرول موجود
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            // إنشاء المستخدم
            var user = new AppUser
            {
                FullName = dto.FullName,
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            // إضافة الرول
            await _userManager.AddToRoleAsync(user, dto.Role);

            // توليد التوكن
            var token = _jwtHelper.GenerateToken(Guid.Parse(user.Id), dto.Role);

            return new RegisterResponse
            {
                Token = token,
                UserId = user.Id
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var token = _jwtHelper.GenerateToken(Guid.Parse(user.Id), role);

            return new AuthResponse { Token = token };
        }
    }
}
