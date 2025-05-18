using AmsApi.DTOs;
using AmsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AmsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // POST /admins/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var response = await _adminService.LoginAsync(dto); // استخدمنا الدالة من الـ service هنا
                return Ok(response);
            }
            catch (Exception)
            {
                return Unauthorized("Invalid credentials");
            }
        }
    }
}
