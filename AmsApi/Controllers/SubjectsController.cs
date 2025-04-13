[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    // GET /api/subjects
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _subjectService.GetAllAsync();
        return Ok(result);
    }

    // GET /api/subjects/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _subjectService.GetByIdAsync(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // POST /api/subjects
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubjectDto dto)
    {
        var result = await _subjectService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // PATCH /api/subjects/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectDto dto)
    {
        var updated = await _subjectService.UpdateAsync(id, dto);
        if (!updated) return NotFound();
        return NoContent();
    }

    // DELETE /api/subjects/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _subjectService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
    [HttpPost("{subjectId}/attendees/{attendeeId}")]
    public async Task<IActionResult> AddAttendee(int subjectId, int attendeeId)
    {
        var result = await _subjectService.AddAttendeeToSubject(subjectId, attendeeId);
        if (!result) return NotFound();
        return Ok(new { message = "Attendee added to subject." });
    }

    [HttpDelete("{subjectId}/attendees/{attendeeId}")]
    public async Task<IActionResult> RemoveAttendee(int subjectId, int attendeeId)
    {
        var result = await _subjectService.RemoveAttendeeFromSubject(subjectId, attendeeId);
        if (!result) return NotFound();
        return Ok(new { message = "Attendee removed from subject." });
    }

    [HttpPost("{subjectId}/instructors/{instructorId}")]
    public async Task<IActionResult> AddInstructor(int subjectId, int instructorId)
    {
        var result = await _subjectService.AddInstructorToSubject(subjectId, instructorId);
        if (!result) return NotFound();
        return Ok(new { message = "Instructor added to subject." });
    }

    [HttpDelete("{subjectId}/instructors/{instructorId}")]
    public async Task<IActionResult> RemoveInstructor(int subjectId, int instructorId)
    {
        var result = await _subjectService.RemoveInstructorFromSubject(subjectId, instructorId);
        if (!result) return NotFound();
        return Ok(new { message = "Instructor removed from subject." });
    }


}
