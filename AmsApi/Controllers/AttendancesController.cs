
namespace AmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendancesController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendancesController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    // GET /api/attendances/subjects/{subjectId}
    [HttpGet("subjects/{subjectId}")]
    public async Task<IActionResult> GetBySubject(int subjectId)
    {
        var result = await _attendanceService.GetAttendanceBySubjectId(subjectId);
        return Ok(result);
    }

    // PUT /api/attendances/subjects/{subjectId}/attendees/{attendeeId}
    [HttpPut("subjects/{subjectId}/attendees/{attendeeId}")]
    public async Task<IActionResult> Mark(int subjectId, int attendeeId, MarkAttendanceDto dto)
    {
        var success = await _attendanceService.MarkAttendance(subjectId, attendeeId, dto);
        if (!success) return BadRequest("Attendance marking failed");

        return Ok(new { message = "Attendance updated successfully" });
    }
}
