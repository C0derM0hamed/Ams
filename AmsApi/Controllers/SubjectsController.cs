using System.Security.Claims;
using AmsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize] // يحمي كل الأكشنات – الدور يتحدد في كل واحدة على حدة
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService SubjectService;
        public SubjectsController(ISubjectService svc) => SubjectService = svc;

        // GET /api/subjects (Admin فقط)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var subjects = await SubjectService.GetAllAsync();
            return Ok(subjects);
        }

        // POST /api/subjects (Admin فقط)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSubjectDto dto)
        {
            var subj = await SubjectService.CreateAsync(dto);
            return StatusCode(201);
        }

        // GET /api/subjects/{subjectId} (متاح للجميع)
        [HttpGet("{subjectId:guid}")]
        public async Task<IActionResult> GetOne(Guid subjectId)
        {
            var subj = await SubjectService.GetByIdAsync(subjectId);
            if (subj == null) return NotFound();
            return Ok(subj);
        }

        // PATCH /api/subjects/{subjectId} (Admin فقط)
        [HttpPatch("{subjectId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid subjectId, [FromBody] UpdateSubjectDto dto)
        {
            var updated = await SubjectService.UpdateAsync(subjectId, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE /api/subjects/{subjectId} (Admin فقط)
        [HttpDelete("{subjectId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid subjectId)
        {
            var ok = await SubjectService.DeleteAsync(subjectId);
            if (!ok) return NotFound();
            return NoContent();
        }

        // GET /api/subjects/{subjectId}/attendees (Admin و Instructor فقط)
        [HttpGet("{subjectId:guid}/attendees")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetAttendees(Guid subjectId)
        {
            var list = await SubjectService.GetAttendeesAsync(subjectId);
            return Ok(list);
        }

        // POST /api/subjects/{subjectId}/subject_dates (Admin فقط)
        [HttpPost("{subjectId:guid}/subject_dates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddDate(Guid subjectId, [FromBody] CreateSubjectDateDto dto)
        {
            var sd = await SubjectService.AddSubjectDateAsync(subjectId, dto);
            return CreatedAtAction(null, new { subjectId = subjectId, subjectDateId = sd.Id }, sd);
        }

        // DELETE /api/subjects/{subjectId}/subject_dates/{subjectDateId} (Admin فقط)
        [HttpDelete("{subjectId:guid}/subject_dates/{subjectDateId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveDate(Guid subjectId, Guid subjectDateId)
        {
            var ok = await SubjectService.RemoveSubjectDateAsync(subjectId, subjectDateId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
