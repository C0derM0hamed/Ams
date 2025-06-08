using AmsApi.Models;
using AmsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmsApi.Controllers;

[ApiController]
[Route("[controller]")]
public class FaceCheckInController : ControllerBase
{
    private readonly FaceRecognitionService _faceService;

    public FaceCheckInController(FaceRecognitionService faceService)
    {
        _faceService = faceService;
    }
    [HttpPost("detect-face")]
    public async Task<IActionResult> DetectFace(IFormFile image, [FromServices] FaceRecognitionService service)
    {
        using var stream = image.OpenReadStream();
        var result = await service.ClassifyAsync(stream, image.FileName);
        return Ok(result);
    }

}


