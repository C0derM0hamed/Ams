
using Microsoft.EntityFrameworkCore;

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
    [HttpGet("report")]
    public async Task<IActionResult> GetAttendanceReport(int subjectId, DateTime date)
    {
        var report = await _attendanceService.GetAttendanceReportAsync(subjectId, date);
        if (report is null)
            return NotFound("Subject not found.");

        return Ok(report);
    }
    [HttpGet("report/pdf")]
    public async Task<IActionResult> DownloadAttendanceReport(int subjectId, DateTime date)
    {
        var report = await _attendanceService.GetAttendanceReportAsync(subjectId, date);
        if (report == null)
            return NotFound("Subject not found or no attendees.");

        var pdfBytes = _attendanceService.GenerateAttendancePdf(
    report.Subject, report.Date, report.Present, report.Absent
);

        return File(pdfBytes, "application/pdf", $"Attendance_{report.Subject}_{report.Date:yyyy-MM-dd}.pdf");
    }


}
