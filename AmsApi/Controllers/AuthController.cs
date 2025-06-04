using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AmsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
        {
            var userId = await _userService.RegisterUserAsync(dto);
            return Ok(new { message = "User created", userId });
        }
        // POST /auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _userService.LoginAsync(dto);
                return Ok(result);
            }
            catch
            {
                return Unauthorized("Invalid credentials");
            }
        }
    }
}
