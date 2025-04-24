
namespace AmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfigService _configService;

    public ConfigController(IConfigService configService)
    {
        _configService = configService;
    }

    // POST /api/config/classifier
    [HttpPost("classifier")]
    public async Task<IActionResult> TrainClassifier()
    {
        var result = await _configService.TrainClassifierAsync();
        if (!result)
            return StatusCode(500, new { message = "Failed to train classifier" });

        return Ok(new { message = "Classifier trained successfully" });
    }

    // PUT /api/config/face_recognition?enabled=true
    [HttpPut("face_recognition")]
    public async Task<IActionResult> ToggleFaceRecognition([FromQuery] bool enabled)
    {
        var result = await _configService.ToggleFaceRecognitionAsync(enabled);
        if (!result)
            return StatusCode(500, new { message = "Failed to update face recognition setting" });

        return Ok(new
        {
            message = $"Face recognition {(enabled ? "enabled" : "disabled")}"
        });
    }

    // GET /api/config/face_recognition
    [HttpGet("face_recognition")]
    public IActionResult GetStatus()
    {
        var isEnabled = _configService.IsFaceRecognitionEnabled();
        return Ok(new { faceRecognition = isEnabled });
    }

    [HttpPost("upload-dataset")]
    public async Task<IActionResult> UploadDataset()
    {
        var success = await _configService.UploadDatasetAsync();
        if (!success)
            return StatusCode(500, "❌ Failed to upload dataset");

        return Ok(new { message = "✅ Dataset uploaded successfully to Python service" });
    }


}
