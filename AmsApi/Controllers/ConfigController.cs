// ConfigController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly FaceRecModeService _modeSvc;
    private readonly IPythonClassifierClient _pyClient;

    public ConfigController(
        FaceRecModeService modeSvc,
        IPythonClassifierClient pyClient)
    {
        _modeSvc = modeSvc;
        _pyClient = pyClient;
    }

    /// <summary>
    /// PUT /api/config/face_recognition?mode=Embed
    /// Switches between "Embed" and "Classify" modes in memory.
    /// </summary>
    [HttpPut("face_recognition")]
    [Authorize(Roles = "Admin")]
    public IActionResult SetMode([FromQuery] FaceRecModeDto dto)
    {
        _modeSvc.SetMode(dto.Mode);
        return Ok(new { message = $"Face-rec mode set to {dto.Mode}" });
    }


    [HttpPost("classifier")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadClassifier([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        await _pyClient.UploadClassifierAsync(file);
        return Ok(new { message = "Classifier updated on Python server" });
    }
}
