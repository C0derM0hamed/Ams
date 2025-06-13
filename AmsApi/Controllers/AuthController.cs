using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
            try
            {
                var result = await _userService.RegisterUserAsync(dto);
                return Ok(new
                {
                    message = "User created",
                    userId = result.UserId,
                    token = result.Token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Something went wrong",
                    error = ex.Message
                });
            }
        }
        [HttpPost("auto-register/{id}")]
        public async Task<IActionResult> AutoRegister(Guid id)
        {
            try
            {
                var result = await _userService.AutoRegisterAsync(id);

                return Ok(new
                {
                    message = "User registered successfully",
                    userId = result.UserId,
                    token = result.Token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
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
