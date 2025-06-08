using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AmsApi.DTOs;
using AmsApi.Helpers;
using AmsApi.Interfaces;
using Azure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize] 
    public class AttendancesController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ISubjectService _subjectService;

        public AttendancesController(
            IAttendanceService attendanceService,
            ISubjectService subjectService)
        {
            _attendanceService = attendanceService;
            _subjectService = subjectService;
        }

        // GET /attendances/subjects/{subjectId}
        [HttpGet("subjects/{subjectId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetAllForSubject(Guid subjectId)
        {
            var subject = await _subjectService.GetByIdAsync(subjectId);

            var list = await _attendanceService.GetBySubjectAsync(subjectId);
            return Ok(list);
        }

        // PUT /attendances/subjects/{subjectId}/attendees/{attendeeId}
        [HttpPut("subjects/{subjectId}/attendees/{attendeeId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CreateOne(Guid subjectId, Guid attendeeId)
        {
            try
            {
                var subject = await _subjectService.GetByIdAsync(subjectId);
                if (subject is null)
                    return BadRequest("subject Not found");
                var attendance = await _attendanceService.CreateOneAsync(subjectId, attendeeId);
                return Ok(attendance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /attendances/subjects/{subjectId}
        [HttpPost("subjects/{subjectId}/attendees")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMany(Guid subjectId, [FromBody] List<Guid> attendeeIds)
        {
            var dto = new CreateManyAttendanceDto
            {
                SubjectId = subjectId,
                AttendeeIds = attendeeIds
            };

            var result = await _attendanceService.CreateManyAsync(dto);
            return Ok(result);
        }

        // DELETE /attendances/{attendanceId}
        [HttpDelete("{attendanceId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> DeleteOne(Guid attendanceId)
        {
            var attendance = await _attendanceService.GetByIdAsync(attendanceId);
            if (attendance == null)
                return NotFound();

            var ok = await _attendanceService.DeleteAsync(attendanceId);
            if (!ok) return NotFound();
            return NoContent();
        }
        [HttpDelete("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAll()
        {
            await _attendanceService.DeleteAllAsync();
            return NoContent(); // 204: successful with no body
        }

        [HttpDelete("subjects/{subjectId}/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllForSubject(Guid subjectId)
        {
            await _attendanceService.DeleteAllForSubjectAsync(subjectId);
            return NoContent(); // 204
        }

        [HttpGet("report/{subjectId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetAttendanceReport(Guid subjectId)
        {
            var report = await _attendanceService.GenerateReportAsync(subjectId);
            return Ok(report);
        }

        [HttpGet("calendar")]
        [Authorize(Roles = "Admin,Instructor,Attendee")]
        public async Task<IActionResult> GetCalendar()
        {
            var dates = await _attendanceService.GetCalendarDatesAsync();
            return Ok(dates);
        }

        [HttpPost("face-checkin")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CheckInByFace([FromForm] IFormFile image, [FromQuery] Guid subjectId)
        {
            try
            {
                var attendance = await _attendanceService.CreateByFaceAsync(image, subjectId);
                return Ok(attendance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error", error = ex.Message });
            }
        }


    }

}
