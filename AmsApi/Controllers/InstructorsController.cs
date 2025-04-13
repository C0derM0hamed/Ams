
namespace AmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _instructorService;
    private readonly IMapper _mapper;

    public InstructorsController(IInstructorService instructorService, IMapper mapper)
    {
        _instructorService = instructorService;
        _mapper = mapper;
    }

    // POST: /api/instructors/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterInstructorDto dto)
    {
        var instructor = await _instructorService.RegisterAsync(dto);
        var result = _mapper.Map<InstructorDto>(instructor);
        return Ok(result);
    }

    // POST: /api/instructors/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginInstructorDto dto)
    {
        var token = await _instructorService.LoginAsync(dto);
        return Ok(token);
    }

    // GET: /api/instructors/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var instructor = await _instructorService.GetByIdAsync(id);
        if (instructor is null) return NotFound();

        var result = _mapper.Map<InstructorDto>(instructor);
        return Ok(result);
    }

    // PUT: /api/instructors/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateInstructorDto dto)
    {
        var updated = await _instructorService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();

        var result = _mapper.Map<InstructorDto>(updated);
        return Ok(result);
    }

    // DELETE: /api/instructors/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _instructorService.DeleteAsync(id);
        if (!deleted) return NotFound();

        return NoContent();
    }

    // POST: /api/instructors/{id}/upload-image
    [HttpPost("{id}/upload-image")]
    public async Task<IActionResult> UploadImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var instructor = await _instructorService.UpdateAsync(id, new UpdateInstructorDto());

        if (instructor == null) return NotFound();

        instructor.ImagePath = $"/images/{fileName}";
        await _instructorService.UpdateAsync(id, new UpdateInstructorDto()); // نحفظ الصورة

        return Ok(new
        {
            message = "Image uploaded successfully",
            imageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}"
        });
    }
}
