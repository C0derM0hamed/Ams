using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace AmsApi.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtHelper _jwtHelper;
        private readonly AmsDbContext _dbContext;

        public UserService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IJwtHelper jwtHelper, AmsDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtHelper = jwtHelper;
            _dbContext = dbContext;
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
        public async Task<RegisterResponse> AutoRegisterAsync(Guid id)
        {
            // الأول شوف لو ID ده لطالب
            var attendee = await _dbContext.Attendees.FindAsync(id);
            if (attendee != null)
            {
                var existingUser = await _userManager.FindByEmailAsync(attendee.Email);
                if (existingUser != null)
                    throw new Exception("User already registered");

                var dto = new CreateUserDto
                {
                    FullName = attendee.FullName,
                    Email = attendee.Email,
                    Password = attendee.Password,
                    Role = "Attendee"
                };

                return await RegisterUserAsync(dto);
            }

            // شوف لو ID ده لمدرّس
            var instructor = await _dbContext.Instructors.FindAsync(id);
            if (instructor != null)
            {
                var existingUser = await _userManager.FindByEmailAsync(instructor.Email);
                if (existingUser != null)
                    throw new Exception("User already registered");

                var dto = new CreateUserDto
                {
                    FullName = instructor.FullName,
                    Email = instructor.Email,
                    Password = instructor.Password,
                    Role = "Instructor"
                };

                return await RegisterUserAsync(dto);
            }

            throw new Exception("No Attendee or Instructor found with this ID");
        }


    }
}
