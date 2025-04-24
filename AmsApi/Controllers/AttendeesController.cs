using System.Text.RegularExpressions;
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

        var attendee = await _attendeeService.GetByIdAsync(id);
        if (attendee == null)
            return NotFound("Attendee not found.");

        var sanitizedFullName = attendee.FullName.Trim().ToLower().Replace(" ", "_");
        sanitizedFullName = Regex.Replace(sanitizedFullName, @"[^a-z0-9_]", "");
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "dataset", $"attendee_{attendee.Id}");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"{sanitizedFullName}{fileExtension}";

        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // تحديث فقط لمسار الصورة
        attendee.ImagePath = $"/dataset/attendee_{attendee.Id}/{fileName}";

        await _attendeeService.UpdateAsync(id, new UpdateAttendeeDto
        {
            FullName = attendee.FullName,
            Email = attendee.Email,
            Password = attendee.Password
        });

        // ✅ هنا بنستخدم AutoMapper عشان يشتغل الريسولفر
        var result = _mapper.Map<AttendeeDto>(attendee);

        return Ok(new
        {
            message = "Image uploaded successfully",
            imageUrl = result.ImageUrl
        });
    }




}
