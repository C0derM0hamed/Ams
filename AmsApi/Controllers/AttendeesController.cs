using AmsApi.Models;
using AmsApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Security.Claims;

namespace AmsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AttendeesController : ControllerBase
    {
        private readonly IAttendeeService _attendeeService;
        private readonly IFaceRecognizer _faceRecognizer;
        private readonly IJwtHelper _jwtHelper;
        private readonly string _pythonUrl;

        public AttendeesController(
            IAttendeeService attendeeService,
            IFaceRecognizer faceRecognizer,
            IJwtHelper jwtHelper,IConfiguration config)
        {
            _attendeeService = attendeeService;
            _faceRecognizer = faceRecognizer;
            _jwtHelper = jwtHelper;
            _pythonUrl = config["PythonFaceRec:BaseUrl"]!;
        }

        public enum FaceRecognitionMode
        {
            Embed,
            Classify
        }

        [HttpGet("test-auth")]
        [Authorize(Roles = "Admin")]
        public IActionResult TestAuth()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginWithCredentials([FromBody] LoginDto payload)
        {
            var attendee = await _attendeeService.GetByEmailAsync(payload.Username);
            if (attendee == null || attendee.Password != payload.Password)
                return Unauthorized(new { message = "Invalid credentials" });

            var token = _jwtHelper.GenerateToken(attendee.Id, "Attendee");
            return Ok(new { token });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var attendees = await _attendeeService.GetAllAsync(); // بيرجع List<AttendeeSummaryDto>
            return Ok(attendees);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOne(Guid id)
        {
            var attendee = await _attendeeService.GetByIdAsync(id); // بيرجع AttendeeDetailsDto
            if (attendee == null)
                return NotFound();

            return Ok(attendee);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateAttendeeDto dto)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var attendee = await _attendeeService.CreateAsync(dto, Guid.Parse(adminId));
            return CreatedAtAction(nameof(GetOne), new { id = attendee.Id }, attendee);
        }

   [HttpPost("{attendee_id}/image")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> UploadImage(Guid attendee_id, [FromForm] IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest(new { message = "No image file uploaded" });

    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);

    try
    {
        await _attendeeService.UploadImageAsync(attendee_id, ms.ToArray());

                var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{attendee_id}/profile.png";


                return Ok(new
        {
            message = "Image uploaded successfully",
            imageUrl
        });
    }
    catch (Exception ex)
    {
        return NotFound(new { message = ex.Message });
    }
}



        [HttpPatch("{attendee_id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid attendee_id, [FromBody] UpdateAttendeeDto dto)
        {
            var updatedAttendee = await _attendeeService.UpdateAsync(attendee_id, dto);
            if (updatedAttendee == null)
                return NotFound();

            return Ok(updatedAttendee);
        }

        [HttpDelete("{attendee_id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid attendee_id)
        {
            var success = await _attendeeService.DeleteAsync(attendee_id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{attendee_id}/subjects/{subject_id}")]
        public async Task<IActionResult> GetOneSubjectForOne(Guid attendee_id, Guid subject_id)
        {
            var role = User.FindFirst("role")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (role != "Admin" && userId != attendee_id.ToString())
                return Unauthorized(new { message = "Unauthorized access" });

            var subject = await _attendeeService.GetSubjectForAttendee(attendee_id, subject_id);
            if (subject == null)
                return NotFound();

            return Ok(subject);
        }

        [HttpGet("{attendee_id}/subjects")]
        public async Task<IActionResult> GetAllSubjectsForAttendee(Guid attendee_id)
        {
            var role = User.FindFirst("role")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (role == "Attendee" && userId != attendee_id.ToString())
                return Unauthorized(new { message = "Unauthorized access" });

            var subjects = await _attendeeService.GetSubjectsForAttendeeAsync(attendee_id);
            return Ok(new { message = "Retrieved associated subjects successfully", subjects });
        }

        [HttpPut("{attendee_id}/subjects/{subject_id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutSubjectToAttendee(Guid attendee_id, Guid subject_id)
        {
            var success = await _attendeeService.AddSubjectToAttendee(attendee_id, subject_id);
            if (!success)
                return NotFound();

            return Ok(new { message = "Subject added to attendee successfully" });
        }

        [HttpDelete("{attendee_id}/subjects/{subject_id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOneSubjectFromOne(Guid attendee_id, Guid subject_id)
        {
            var success = await _attendeeService.RemoveSubjectFromAttendee(attendee_id, subject_id);
            if (!success)
                return NotFound(new { message = "Subject not found or already removed from attendee" });

            return Ok(new { message = "Subject was removed from attendee successfully" });
        }

        [HttpPost("{attendee_id}/upload_images_for_training")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImagesForTraining(Guid attendee_id, [FromForm] List<IFormFile> files)
        {
            var attendee = await _attendeeService.GetByIdAsync(attendee_id);
            if (attendee == null)
                return NotFound("Attendee not found.");

            if (files == null || !files.Any())
                return BadRequest("No images uploaded.");

            var studentNumber = attendee.Number.ToString();
            var tempPath = Path.GetTempPath();
            var zipPath = Path.Combine(tempPath, $"{studentNumber}_training.zip");

            if (System.IO.File.Exists(zipPath))
                System.IO.File.Delete(zipPath);
            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    var filePath = Path.Combine(tempPath, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    archive.CreateEntryFromFile(filePath, file.FileName);
                    System.IO.File.Delete(filePath);
                }

                var numberPath = Path.Combine(tempPath, "student_number.txt");
                await System.IO.File.WriteAllTextAsync(numberPath, studentNumber);
                archive.CreateEntryFromFile(numberPath, "student_number.txt");
                System.IO.File.Delete(numberPath);
            }

            using var client = new HttpClient();
            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(await System.IO.File.ReadAllBytesAsync(zipPath)), "file", $"{studentNumber}_training.zip");

            var response = await client.PostAsync($"{_pythonUrl}/upload_training_images", content);
            var result = await response.Content.ReadAsStringAsync();

            System.IO.File.Delete(zipPath);

            return Ok(result);
        }
    }
}
