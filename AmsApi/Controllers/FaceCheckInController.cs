using AmsApi.Models;
using AmsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaceCheckInController : ControllerBase
{
    private readonly FaceRecognitionService _faceService;
    private readonly AmsDbContext _context;

    public FaceCheckInController(FaceRecognitionService faceService, AmsDbContext context)
    {
        _faceService = faceService;
        _context = context;
    }

    [HttpPost("check")]
    public async Task<IActionResult> CheckFace(
     [FromForm] IFormFile image,
     [FromForm] int subjectId
 )
    {
        if (image == null || image.Length == 0)
            return BadRequest("Image is required.");

        var predictedName = await _faceService.ClassifyFaceAsync(image);
        if (string.IsNullOrWhiteSpace(predictedName))
            return StatusCode(500, "Face classification failed.");

        var matchedAttendee = await _context.Attendees
            .FirstOrDefaultAsync(a => a.FullName.ToLower() == predictedName.ToLower());

        if (matchedAttendee == null)
            return NotFound("Student not found.");

        // هل هو فعلاً مسجل في المادة؟
        if (!matchedAttendee.SubjectIds.Contains(subjectId))
            return BadRequest("Student not registered in this subject.");

        var today = DateTime.UtcNow.Date;

        var existingAttendance = await _context.Attendances
            .FirstOrDefaultAsync(a =>
                a.SubjectId == subjectId &&
                a.AttendeeId == matchedAttendee.Id &&
                a.Date.Date == today);

        if (existingAttendance == null)
        {
            _context.Attendances.Add(new Attendance
            {
                SubjectId = subjectId,
                AttendeeId = matchedAttendee.Id,
                Date = today,
                IsPresent = true
            });

            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            message = "Attendance marked successfully",
            student = matchedAttendee.FullName,
            date = today.ToString("yyyy-MM-dd")
        });
    }


}
