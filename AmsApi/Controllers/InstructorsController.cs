using AmsApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace AmsApi.Controllers;

[ApiController]
[Route("[controller]")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _service;

    public InstructorsController(IInstructorService service)
    {
        _service = service;
    }

    // GET /api/instructors
    [HttpGet]
    public async Task<IActionResult> GetAll([FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        if (principal == null || role != "Admin")
            return Unauthorized();

        var list = await _service.GetAllAsync();
        return Ok(list);
    }

    // POST /api/instructors
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInstructorDto dto, [FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        if (principal == null || role != "Admin")
            return Unauthorized();

        var inst = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetOne), new { instructorId = inst.Id }, inst);
    }

    // POST /api/instructors/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto payload)
    {
        var inst = await _service.GetByEmailAsync(payload.Username);
        if (inst == null || inst.Password != payload.Password)
            return Unauthorized();

        var token = JwtHelper.GenerateToken(inst.Id, "Instructor");
        return Ok(new { token });
    }

    // GET /api/instructors/login
    [HttpGet("login")]
    public async Task<IActionResult> LoginWithToken([FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (principal == null || role != "Instructor")
            return Unauthorized();

        var inst = await _service.GetByIdAsync(Guid.Parse(userId!));
        if (inst == null) return NotFound();
        return Ok(inst);
    }

    // GET /api/instructors/{instructorId}
    [HttpGet("{instructorId:guid}")]
    public async Task<IActionResult> GetOne(Guid instructorId, [FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (principal == null ||
            (role != "Admin" && userId != instructorId.ToString()))
            return Unauthorized();

        var inst = await _service.GetByIdAsync(instructorId);
        if (inst == null) return NotFound();
        return Ok(inst);
    }

    // PATCH /api/instructors/{instructorId}
    [HttpPatch("{instructorId:guid}")]
    public async Task<IActionResult> Update(Guid instructorId, [FromBody] UpdateInstructorDto dto, [FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        if (principal == null || role != "Admin")
            return Unauthorized();

        var updated = await _service.UpdateAsync(instructorId, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    // DELETE /api/instructors/{instructorId}
    [HttpDelete("{instructorId:guid}")]
    public async Task<IActionResult> Delete(Guid instructorId, [FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        if (principal == null || role != "Admin")
            return Unauthorized();

        var ok = await _service.DeleteAsync(instructorId);
        if (!ok) return NotFound();
        return NoContent();
    }

    // POST /api/instructors/{instructorId}/image
    [HttpPost("{instructorId:guid}/image")]
    public async Task<IActionResult> UploadImage(Guid instructorId, IFormFile file, [FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        if (principal == null || role != "Admin")
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest("No file");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var path = await _service.UploadImageAsync(instructorId, ms.ToArray());
        return Ok(new { imagePath = path });
    }

    // GET /api/instructors/{instructorId}/subjects
    [HttpGet("{instructorId:guid}/subjects")]
    public async Task<IActionResult> GetSubjects(Guid instructorId, [FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (principal == null ||
            (role != "Admin" && userId != instructorId.ToString()))
            return Unauthorized();

        var list = await _service.GetSubjectsForInstructorAsync(instructorId);
        return Ok(list);
    }

    // GET /api/instructors/{instructorId}/subjects/{subjectId}
    [HttpGet("{instructorId:guid}/subjects/{subjectId:guid}")]
    public async Task<IActionResult> GetSubject(Guid instructorId, Guid subjectId, [FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (principal == null ||
            (role != "Admin" && userId != instructorId.ToString()))
            return Unauthorized();

        var subj = await _service.GetSubjectForInstructorAsync(instructorId, subjectId);
        if (subj == null) return NotFound();
        return Ok(subj);
    }

    // PUT /api/instructors/{instructorId}/subjects/{subjectId}
    [HttpPut("{instructorId:guid}/subjects/{subjectId:guid}")]
    public async Task<IActionResult> AssignSubject(Guid instructorId, Guid subjectId, [FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        if (principal == null || role != "Admin")
            return Unauthorized();

        var ok = await _service.AssignSubjectToInstructorAsync(instructorId, subjectId);
        if (!ok) return NotFound();
        return Ok();
    }

    // DELETE /api/instructors/{instructorId}/subjects/{subjectId}
    [HttpDelete("{instructorId:guid}/subjects/{subjectId:guid}")]
    public async Task<IActionResult> UnassignSubject(Guid instructorId, Guid subjectId, [FromHeader] string jwtToken)
    {
        var principal = JwtHelper.ValidateToken(jwtToken);
        var role = principal.FindFirst("role")?.Value;
        if (principal == null || role != "Admin")
            return Unauthorized();

        var ok = await _service.RemoveSubjectFromInstructorAsync(instructorId, subjectId);
        if (!ok) return NotFound();
        return NoContent();
    }
}

