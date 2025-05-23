using AmsApi.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AmsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AttendeesController : ControllerBase
    {
        private readonly IAttendeeService _attendeeService;
        private readonly IFaceRecognizer _faceRecognizer;

        public AttendeesController(IAttendeeService attendeeService, IFaceRecognizer faceRecognizer)
        {
            _attendeeService = attendeeService;
            _faceRecognizer = faceRecognizer;
        }
        public enum FaceRecognitionMode
        {
            Embed,     // طريقة التضمين
            Classify   // طريقة التصنيف
        }
        // Login with credentials
        [HttpPost("login")]
        public async Task<IActionResult> LoginWithCredentials([FromBody] LoginDto payload)
        {
            var attendee = await _attendeeService.GetByEmailAsync(payload.Username);
            if (attendee == null || attendee.Password != payload.Password)
                return Unauthorized(new { message = "Invalid credentials" });

            var token = JwtHelper.GenerateToken(attendee.Id, "Attendee");

            return Ok(new { token });
        }

        // Login with token
        [HttpGet("login")]
        public async Task<IActionResult> LoginWithToken([FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var adminId = claims?.FindFirst("adminId")?.Value;
            var UserId = claims?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (claims == null || adminId != "Attendee")
                return Unauthorized(new { message = "Unauthorized access" });

            var attendee = await _attendeeService.GetByIdAsync(Guid.Parse(UserId));
            if (attendee == null)
                return NotFound();

            return Ok(attendee);
        }
        // Get all attendees
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var adminId = claims?.FindFirst("adminId")?.Value;
            if (claims == null || adminId ==null)
                return Unauthorized(new { message = "Unauthorized access" });

            var attendees = await _attendeeService.GetAllAsync();
            return Ok(attendees);
        }
        // Get attendee by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(Guid id, [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            
            var adminId = claims?.FindFirst("adminId")?.Value;
            var UserId = claims?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (claims == null || (adminId ==null && UserId != id.ToString()))
                return Unauthorized(new { message = "Unauthorized access" });

            var attendee = await _attendeeService.GetByIdAsync(id);
            if (attendee == null)
                return NotFound();

            return Ok(attendee);
        }

        // Create attendee
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAttendeeDto dto, [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var adminId = claims?.FindFirst("adminId")?.Value;

            if (claims == null||adminId==null)
                return Unauthorized(new { message = "Unauthorized access" });

            var attendee = await _attendeeService.CreateAsync(dto, Guid.Parse(adminId));
            return CreatedAtAction(nameof(GetOne), new { id = attendee.Id }, attendee);
        }

        // Endpoint to upload image for face recognition
        [HttpPost("image")]
        public async Task<IActionResult> GetAllWithImage([FromForm] IFormFile file, [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var adminId = claims?.FindFirst("adminId")?.Value;

            // If the user is an Attendee, deny access
            if (adminId == null)
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }

            // Check if the file is empty
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No image file uploaded" });
            }

            // Read the file as a byte array
            var imageBytes = await ReadFileAsync(file);

            // Fetch all attendees
            var attendees = await _attendeeService.GetAllAsync();

            // Using face recognition to either embed or classify the image
            var attendeesWithImage = await ProcessImageForFaceRecognition(attendees, imageBytes);

            return Ok(new { message = "Retrieved all attendees successfully", attendees = attendeesWithImage });
        }

        // Method to read image from the file and return as byte[]
        private async Task<byte[]> ReadFileAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        // Process the image based on Face Recognition mode
        private async Task<List<Attendee>> ProcessImageForFaceRecognition(List<Attendee> attendees, byte[] image)
        {
            var resultAttendees = new List<Attendee>();

            var faceRecognitionMode = FaceRecognitionMode.Embed;  // This can be dynamically set based on your settings

            switch (faceRecognitionMode)
            {
                case FaceRecognitionMode.Embed:
                    var embedding = await _faceRecognizer.EmbedAsync(image);
                    resultAttendees = attendees
                        .Where(attendee => attendee.Embedding != null)
                        .Where(attendee =>
                        {
                            var distance = GetDistance(attendee.Embedding, embedding);
                            return distance < 0.6;  // A threshold for matching
                        })
                        .ToList();
                    break;

                case FaceRecognitionMode.Classify:
                    var classId = await _faceRecognizer.ClassifyAsync(image);
                    resultAttendees = attendees
                        .Where(attendee => attendee.Id == classId)
                        .ToList();
                    break;
            }

            return resultAttendees;
        }

        // A helper function to calculate the distance between embeddings
        private double GetDistance(double[] embedding1, double[] embedding2)
        {
            // Placeholder logic to calculate distance between two embeddings.
            // Implement actual distance calculation algorithm (e.g., Euclidean distance)
            return embedding1.Zip(embedding2, (x, y) => Math.Pow(x - y, 2)).Sum();
        }

      
       
        // Upload image for an attendee
        [HttpPost("{attendee_id}/image")]
        public async Task<IActionResult> UploadImage(Guid attendee_id, [FromForm] IFormFile file, [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var UserRole = claims.FindFirst("role")?.Value;

            if (claims == null || UserRole == "Attendee")
                return Unauthorized(new { message = "Unauthorized access" });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No image file uploaded" });

            // Save Image Logic Here
            var imageBytes = await ImageHelper.SaveImageAsync(file);
            await _attendeeService.UploadImageAsync(attendee_id, imageBytes);

            return Ok(new { message = "Image uploaded successfully" });
        }


        

        // Update attendee details
        [HttpPatch("{attendee_id}")]
        public async Task<IActionResult> Update(Guid attendee_id, [FromBody] UpdateAttendeeDto dto, [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            
            var adminId = claims?.FindFirst("adminId")?.Value;
            if (claims == null || adminId == null)
                return Unauthorized(new { message = "Unauthorized access" });

            var updatedAttendee = await _attendeeService.UpdateAsync(attendee_id, dto);
            if (updatedAttendee == null)
                return NotFound();

            return Ok(updatedAttendee);
        }

        // Delete attendee
        [HttpDelete("{attendee_id}")]
        public async Task<IActionResult> Delete(Guid attendee_id, [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var adminId = claims?.FindFirst("adminId")?.Value;
            if (claims == null || adminId == null)
                return Unauthorized(new { message = "Unauthorized access" });

            var success = await _attendeeService.DeleteAsync(attendee_id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        
        // Get specific subject for a specific attendee
        [HttpGet("{attendee_id}/subjects/{subject_id}")]
        public async Task<IActionResult> GetOneSubjectForOne(Guid attendee_id, Guid subject_id, [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var UserRole = claims.FindFirst("role")?.Value;
            var UserId = claims?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (claims == null || (UserRole != "Admin" && UserId != attendee_id.ToString()))
                return Unauthorized(new { message = "Unauthorized access" });

            var subject = await _attendeeService.GetSubjectForAttendee(attendee_id, subject_id);
            if (subject == null)
                return NotFound();

            return Ok(subject);
        }
        // Endpoint لاسترجاع جميع الموضوعات الخاصة بالمتدرب
        [HttpGet("{attendee_id}/subjects")]
        public async Task<IActionResult> GetAllSubjectsForAttendee(Guid attendee_id, [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var userRole = claims?.FindFirst("role")?.Value;

            // التحقق إذا كان المستخدم هو "Attendee" ولم يتطابق معرفه مع معرف المتدرب
            if (userRole == "Attendee" && claims?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value != attendee_id.ToString())
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }

            try
            {
                var subjects = await _attendeeService.GetSubjectsForAttendeeAsync(attendee_id);
                return Ok(new { message = "Retrieved associated subjects successfully", subjects });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Add a subject to a specific attendee
        [HttpPut("{attendee_id}/subjects/{subject_id}")]
        public async Task<IActionResult> PutSubjectToAttendee(Guid attendee_id, Guid subject_id, [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var UserRole = claims.FindFirst("role")?.Value;
            if (claims == null || UserRole != "Admin")
                return Unauthorized(new { message = "Unauthorized access" });

            var success = await _attendeeService.AddSubjectToAttendee(attendee_id, subject_id);
            if (!success)
                return NotFound();

            return Ok(new { message = "Subject added to attendee successfully" });
        }
        [HttpDelete("{attendee_id}/subjects/{subject_id}")]
        public async Task<IActionResult> DeleteOneSubjectFromOne(Guid attendee_id, Guid subject_id, [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var userRole = claims?.FindFirst("role")?.Value;

            // التأكد أن المستخدم هو الـ Admin
            if (userRole != "Admin")
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }

            // إزالة الموضوع من المتدرب
            var success = await _attendeeService.RemoveSubjectFromAttendee(attendee_id, subject_id);
            if (!success)
            {
                return NotFound(new { message = "Subject not found or already removed from attendee" });
            }

            // الرد بنجاح
            return Ok(new { message = "Subject was removed from attendee successfully" });
        }
    }
}
