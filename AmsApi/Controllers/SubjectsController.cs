namespace AmsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _svc;
        public SubjectsController(ISubjectService svc) => _svc = svc;

        // GET /api/subjects
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] string jwtToken)
        {
            var principal = JwtHelper.ValidateToken(jwtToken);
            var role = principal?.FindFirst("role")?.Value;
            if (principal == null || role != "Admin")
                return Unauthorized();
            var list = await _svc.GetAllAsync();
            return Ok(list);
        }

        // POST /api/subjects
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateSubjectDto dto,
            [FromHeader] string jwtToken)
        {
            var principal = JwtHelper.ValidateToken(jwtToken);
            var role = principal?.FindFirst("role")?.Value;
            if (principal == null || role != "Admin")
                return Unauthorized();
            var subj = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetOne), new { subjectId = subj.Id }, subj);
        }

        // GET /api/subjects/{subjectId}
        [HttpGet("{subjectId:guid}")]
        public async Task<IActionResult> GetOne(Guid subjectId)
        {
            var subj = await _svc.GetByIdAsync(subjectId);
            if (subj == null) return NotFound();
            return Ok(subj);
        }

        // PATCH /api/subjects/{subjectId}
        [HttpPatch("{subjectId:guid}")]
        public async Task<IActionResult> Update(
            Guid subjectId,
            [FromBody] UpdateSubjectDto dto,
            [FromHeader] string jwtToken)
        {
            var principal = JwtHelper.ValidateToken(jwtToken);
            var role = principal?.FindFirst("role")?.Value;
            if (principal == null || role != "Admin")
                return Unauthorized();
            var updated = await _svc.UpdateAsync(subjectId, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE /api/subjects/{subjectId}
        [HttpDelete("{subjectId:guid}")]
        public async Task<IActionResult> Delete(
            Guid subjectId,
            [FromHeader] string jwtToken)
        {
            var principal = JwtHelper.ValidateToken(jwtToken);
            var role = principal?.FindFirst("role")?.Value;
            if (principal == null || role != "Admin")
                return Unauthorized();
            var ok = await _svc.DeleteAsync(subjectId);
            if (!ok) return NotFound();
            return NoContent();
        }

        // GET /api/subjects/{subjectId}/attendees
        [HttpGet("{subjectId:guid}/attendees")]
        public async Task<IActionResult> GetAttendees(
            Guid subjectId,
            [FromHeader] string jwtToken)
        {
            var principal = JwtHelper.ValidateToken(jwtToken);
            var role = principal?.FindFirst("role")?.Value;
            if (principal == null || role == "Attendee")
                return Unauthorized();
            var list = await _svc.GetAttendeesAsync(subjectId);
            return Ok(list);
        }

        // POST /api/subjects/{subjectId}/subject_dates
        [HttpPost("{subjectId:guid}/subject_dates")]
        public async Task<IActionResult> AddDate(
            Guid subjectId,
            [FromBody] CreateSubjectDateDto dto,
            [FromHeader] string jwtToken)
        {
            var principal = JwtHelper.ValidateToken(jwtToken);
            var role = principal?.FindFirst("role")?.Value;
            if (principal == null || role != "Admin")
                return Unauthorized();
            var sd = await _svc.AddSubjectDateAsync(subjectId, dto);
            return CreatedAtAction(null, new { subjectId = subjectId, subjectDateId = sd.Id }, sd);
        }

        // DELETE /api/subjects/{subjectId}/subject_dates/{subjectDateId}
        [HttpDelete("{subjectId:guid}/subject_dates/{subjectDateId:guid}")]
        public async Task<IActionResult> RemoveDate(
            Guid subjectId,
            Guid subjectDateId,
            [FromHeader] string jwtToken)
        {
            var principal = JwtHelper.ValidateToken(jwtToken);
            var role = principal?.FindFirst("role")?.Value;
            if (principal == null || role != "Admin")
                return Unauthorized();
            var ok = await _svc.RemoveSubjectDateAsync(subjectId, subjectDateId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
