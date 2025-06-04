using AmsApi.Models;
using AmsApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;

namespace AmsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    
   
    public class AttendeesController : ControllerBase
    {
        private readonly IAttendeeService _attendeeService;
        private readonly IFaceRecognizer _faceRecognizer;
        private readonly IJwtHelper _jwtHelper;

        public AttendeesController(
            IAttendeeService attendeeService,
            IFaceRecognizer faceRecognizer,
            IJwtHelper jwtHelper)
        {
            _attendeeService = attendeeService;
            _faceRecognizer = faceRecognizer;
            _jwtHelper = jwtHelper;
        }

        public enum FaceRecognitionMode
        {
            Embed,
            Classify
        }

        [HttpGet("test-auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            var user = User.Identity;
            return Ok(new { name = user.Name, authenticated = user.IsAuthenticated, claims = User.Claims.Select(c => new { c.Type, c.Value }) });
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

        [HttpGet("login")]
        public async Task<IActionResult> LoginWithToken()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var role = User.FindFirst("role")?.Value;

            if (role != "Attendee" || userId == null)
                return Unauthorized(new { message = "Unauthorized access" });

            var attendee = await _attendeeService.GetByIdAsync(Guid.Parse(userId));
            if (attendee == null)
                return NotFound();

            return Ok(attendee);
        }

        [HttpGet]
        [Authorize(Roles ="Attendee")]
        //[AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var attendees = await _attendeeService.GetAllAsync();
            return Ok(attendees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(Guid id)
        {
            var role = User.FindFirst("role")?.Value;
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (role != "Admin" && userId != id.ToString())
                return Unauthorized(new { message = "Unauthorized access" });

            var attendee = await _attendeeService.GetByIdAsync(id);
            if (attendee == null)
                return NotFound();

            return Ok(attendee);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        //[AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateAttendeeDto dto)
        {
            var adminId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var attendee = await _attendeeService.CreateAsync(dto, Guid.Parse(adminId));
            return CreatedAtAction(nameof(GetOne), new { id = attendee.Id }, attendee);
        }

        [HttpPost("image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllWithImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No image file uploaded" });

            var imageBytes = await ReadFileAsync(file);
            var attendees = await _attendeeService.GetAllAsync();
            var attendeesWithImage = await ProcessImageForFaceRecognition(attendees, imageBytes);

            return Ok(new { message = "Retrieved all attendees successfully", attendees = attendeesWithImage });
        }

        private async Task<byte[]> ReadFileAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        private async Task<List<Attendee>> ProcessImageForFaceRecognition(List<Attendee> attendees, byte[] image)
        {
            var resultAttendees = new List<Attendee>();
            var faceRecognitionMode = FaceRecognitionMode.Embed;

            switch (faceRecognitionMode)
            {
                case FaceRecognitionMode.Embed:
                    var embedding = await _faceRecognizer.EmbedAsync(image);
                    resultAttendees = attendees
                        .Where(a => a.Embedding != null && GetDistance(a.Embedding, embedding) < 0.6)
                        .ToList();
                    break;
                case FaceRecognitionMode.Classify:
                    var classId = await _faceRecognizer.ClassifyAsync(image);
                    resultAttendees = attendees
                        .Where(a => a.Id == classId)
                        .ToList();
                    break;
            }

            return resultAttendees;
        }

        private double GetDistance(double[] e1, double[] e2)
        {
            return e1.Zip(e2, (x, y) => Math.Pow(x - y, 2)).Sum();
        }

        [HttpPost("{attendee_id}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImage(Guid attendee_id, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No image file uploaded" });

            var imageBytes = await ImageHelper.SaveImageAsync(file);
            await _attendeeService.UploadImageAsync(attendee_id, imageBytes);

            return Ok(new { message = "Image uploaded successfully" });
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
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

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
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

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

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    var filePath = Path.Combine(tempPath, file.FileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
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

            var response = await client.PostAsync("https://ba04-156-209-187-195.ngrok-free.app/upload_training_images", content);
            var result = await response.Content.ReadAsStringAsync();

            System.IO.File.Delete(zipPath);

            return Ok(result);
        }
    }
}
