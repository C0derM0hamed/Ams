using AmsApi.Models;
using AmsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaceCheckInController : ControllerBase
{
    private readonly FaceRecognitionService _faceService;
    private readonly AmsDbContext _context;

    public FaceCheckInController(FaceRecognitionService faceService, AmsDbContext context)
    {
        _faceService = faceService;
        _context = context;
    }
    [HttpPost("detect-face")]
    public async Task<IActionResult> DetectFace(IFormFile image, [FromServices] FaceRecognitionService service)
    {
        using var stream = image.OpenReadStream();
        var result = await service.ClassifyAsync(stream, image.FileName);
        return Ok(result);
    }

}


