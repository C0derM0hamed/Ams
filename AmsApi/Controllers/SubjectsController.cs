using System.Security.Claims;
using AmsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize] 
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService SubjectService;
        public SubjectsController(ISubjectService svc) => SubjectService = svc;

        // GET /subjects (Admin فقط)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var subjects = await SubjectService.GetAllAsync();
            return Ok(subjects);
        }

        // POST /subjects (Admin فقط)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSubjectDto dto)
        {
            var subj = await SubjectService.CreateAsync(dto);
            return StatusCode(201);
        }

        // GET /subjects/{subjectId} (متاح للجميع)
        [HttpGet("{subjectId:guid}")]
        public async Task<IActionResult> GetOne(Guid subjectId)
        {
            var subj = await SubjectService.GetByIdAsync(subjectId);
            if (subj == null) return NotFound();
            return Ok(subj);
        }

        // PATCH /subjects/{subjectId} (Admin فقط)
        [HttpPatch("{subjectId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid subjectId, [FromBody] UpdateSubjectDto dto)
        {
            var updated = await SubjectService.UpdateAsync(subjectId, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE /subjects/{subjectId} (Admin فقط)
        [HttpDelete("{subjectId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid subjectId)
        {
            var ok = await SubjectService.DeleteAsync(subjectId);
            if (!ok) return NotFound();
            return NoContent();
        }

        // GET /subjects/{subjectId}/attendees (Admin و Instructor فقط)
        [HttpGet("{subjectId}/attendees")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetAttendees(Guid subjectId)
        {
            var list = await SubjectService.GetAttendeesAsync(subjectId);
            return Ok(list);
        }

        // POST /subjects/{subjectId}/subject_dates (Admin فقط)
        [HttpPost("{subjectId:guid}/subject_dates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddDate(Guid subjectId, [FromBody] CreateSubjectDateDto dto)
        {
            var sd = await SubjectService.AddSubjectDateAsync(subjectId, dto);
            return CreatedAtAction(null, new { subjectId = subjectId, subjectDateId = sd.Id }, sd);
        }
        [HttpGet("{subjectId:guid}/subject_dates")]
        public async Task<IActionResult> GetSubjectDates(Guid subjectId)
        {
            var dates = await SubjectService.GetSubjectDatesAsync(subjectId);
            return Ok(dates);
        }

        // DELETE /subjects/{subjectId}/subject_dates/{subjectDateId} (Admin فقط)
        [HttpDelete("{subjectId:guid}/subject_dates/{subjectDateId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveDate(Guid subjectId, Guid subjectDateId)
        {
            var ok = await SubjectService.RemoveSubjectDateAsync(subjectId, subjectDateId);
            if (!ok) return NotFound();
            return NoContent();
        }
        [HttpDelete("all-subjects")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllSubjects()
        {
            var deletedCount = await SubjectService.DeleteAllAsync();
            return Ok(new { message = $"Deleted {deletedCount} subjects successfully." });
        }
        [HttpDelete("subject-dates")]
        public async Task<IActionResult> DeleteAllSubjectDates()
        {
            var count = await SubjectService.DeleteAllSubjectDatesAsync();
            return Ok(new
            {
                message = $"Deleted {count} subject date(s)"
            });
        }


    }
}
