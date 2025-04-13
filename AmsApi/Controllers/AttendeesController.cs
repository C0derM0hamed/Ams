
namespace AmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendeesController : ControllerBase
{
    private readonly IAttendeeService _attendeeService;
    private readonly IMapper _mapper;
    public AttendeesController(IMapper mapper,IAttendeeService attendeeService)
    {
        _mapper = mapper;
        _attendeeService = attendeeService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterAttendeeDto dto)
    {
        var attendee = await _attendeeService.RegisterAsync(dto);
        var result = _mapper.Map<AttendeeDto>(attendee);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginAttendeeDto dto)
    {
        var result = await _attendeeService.LoginAsync(dto);
        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var attendee = await _attendeeService.GetByIdAsync(id);
        if (attendee is null) return NotFound();

        var result = _mapper.Map<AttendeeDto>(attendee);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _attendeeService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateAttendeeDto dto)
    {
        var updated = await _attendeeService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

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

        var attendee = await _attendeeService.UpdateAsync(id, new UpdateAttendeeDto
        {
            // نحدّث المسار داخل attendee
            // نخزن فقط اسم الملف وليس المسار الكامل
            FullName = null,
            Email = null,
            Password = null
        });

        if (attendee == null) return NotFound();

        attendee.ImagePath = $"/images/{fileName}";

        return Ok(new
        {
            message = "Image uploaded successfully",
            imageUrl = attendee.ImagePath
        });
    }
}
