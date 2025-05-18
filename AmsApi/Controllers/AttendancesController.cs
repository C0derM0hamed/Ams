// AmsApi/Controllers/AttendancesController.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using AmsApi.DTOs;
using AmsApi.Helpers;
using AmsApi.Interfaces;
using AmsApi.Models;
using AmsApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AmsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
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

        // GET /api/attendances/subjects/{subjectId}
        [HttpGet("subjects/{subjectId}")]
        public async Task<IActionResult> GetAllForSubject(
            Guid subjectId,
            [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var role = claims?.FindFirst("role")?.Value;
            var userId = claims?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var subject = await _subjectService.GetByIdAsync(subjectId);
            if (claims == null
                || (role != "Admin"
                    && subject.InstructorId?.ToString() != userId))
                return Unauthorized();

            var list = await _attendanceService.GetBySubjectAsync(subjectId);
            return Ok(list);
        }

        // PUT /api/attendances/subjects/{subjectId}/attendees/{attendeeId}
        [HttpPut("subjects/{subjectId}/attendees/{attendeeId}")]
        public async Task<IActionResult> CreateOne(
            Guid subjectId,
            Guid attendeeId,
            [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var role = claims?.FindFirst("role")?.Value;
            var userId = claims?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var subject = await _subjectService.GetByIdAsync(subjectId);
            if (claims == null
                || (role != "Admin"
                    && subject.InstructorId?.ToString() != userId))
                return Unauthorized();

            var attendance = await _attendanceService.CreateOneAsync(subjectId, attendeeId);
            return Ok(attendance);
        }

        // POST /api/attendances/subjects/{subjectId}
        [HttpPost("subjects/{subjectId}")]
        public async Task<IActionResult> CreateMany(
            Guid subjectId,
            [FromBody] CreateAttendancesDto dto,
            [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var role = claims?.FindFirst("role")?.Value;
            var userId = claims?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var subject = await _subjectService.GetByIdAsync(subjectId);
            if (claims == null
                || (role != "Admin"
                    && subject.InstructorId?.ToString() != userId))
                return Unauthorized();

            var attendances = await _attendanceService.CreateManyAsync(subjectId, dto.AttendeeIds);
            return Ok(attendances);
        }

        // DELETE /api/attendances/{attendanceId}
        [HttpDelete("{attendanceId}")]
        public async Task<IActionResult> DeleteOne(
            Guid attendanceId,
            [FromHeader] string jwtToken)
        {
            var claims = JwtHelper.ValidateToken(jwtToken);
            var role = claims?.FindFirst("role")?.Value;
            var userId = claims?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var attendance = await _attendanceService.GetByIdAsync(attendanceId);
            if (claims == null
                || (role != "Admin"
                    && attendance.Subject.InstructorId?.ToString() != userId))
                return Unauthorized();

            var ok = await _attendanceService.DeleteAsync(attendanceId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
