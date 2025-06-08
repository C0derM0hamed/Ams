using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AmsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet("face-api-url")]
        public async Task<IActionResult> GetUrl()
        {
            var value = await _settingsService.GetValueAsync("PythonFaceRec.BaseUrl");
            return Ok(new { baseUrl = value });
        }

        [HttpPut("face-api-url")]
        public async Task<IActionResult> UpdateUrl([FromBody] string newUrl)
        {
            await _settingsService.UpdateValueAsync("PythonFaceRec.BaseUrl", newUrl);
            return NoContent();
        }
    }
}
