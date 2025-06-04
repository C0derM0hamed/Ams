using AmsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AmsApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _service;
    private readonly IJwtHelper _jwtHelper;

    public InstructorsController(IInstructorService service, IJwtHelper jwtHelper)
    {
        _service = service;
        _jwtHelper = jwtHelper;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateInstructorDto dto)
    {
        var inst = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetOne), new { instructorId = inst.Id }, inst);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto payload)
    {
        var inst = await _service.GetByEmailAsync(payload.Username);
        if (inst == null || inst.Password != payload.Password)
            return Unauthorized();

        var token = _jwtHelper.GenerateToken(inst.Id, "Instructor");
        return Ok(new { token });
    }

    [HttpGet("login")]
    public async Task<IActionResult> LoginWithToken()
    {
        var role = User.FindFirst("role")?.Value;
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (role != "Instructor" || userId == null)
            return Unauthorized();

        var inst = await _service.GetByIdAsync(Guid.Parse(userId));
        if (inst == null) return NotFound();
        return Ok(inst);
    }

    [HttpGet("{instructorId:guid}")]
    public async Task<IActionResult> GetOne(Guid instructorId)
    {
        var role = User.FindFirst("role")?.Value;
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (role != "Admin" && userId != instructorId.ToString())
            return Unauthorized();

        var inst = await _service.GetByIdAsync(instructorId);
        if (inst == null) return NotFound();
        return Ok(inst);
    }

    [HttpPatch("{instructorId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid instructorId, [FromBody] UpdateInstructorDto dto)
    {
        var updated = await _service.UpdateAsync(instructorId, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{instructorId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid instructorId)
    {
        var ok = await _service.DeleteAsync(instructorId);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpPost("{instructorId:guid}/image")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadImage(Guid instructorId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var path = await _service.UploadImageAsync(instructorId, ms.ToArray());
        return Ok(new { imagePath = path });
    }

    [HttpGet("{instructorId:guid}/subjects")]
    public async Task<IActionResult> GetSubjects(Guid instructorId)
    {
        var role = User.FindFirst("role")?.Value;
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (role != "Admin" && userId != instructorId.ToString())
            return Unauthorized();

        var list = await _service.GetSubjectsForInstructorAsync(instructorId);
        return Ok(list);
    }

    [HttpGet("{instructorId:guid}/subjects/{subjectId:guid}")]
    public async Task<IActionResult> GetSubject(Guid instructorId, Guid subjectId)
    {
        var role = User.FindFirst("role")?.Value;
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (role != "Admin" && userId != instructorId.ToString())
            return Unauthorized();

        var subj = await _service.GetSubjectForInstructorAsync(instructorId, subjectId);
        if (subj == null) return NotFound();
        return Ok(subj);
    }

    [HttpPut("{instructorId:guid}/subjects/{subjectId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignSubject(Guid instructorId, Guid subjectId)
    {
        var ok = await _service.AssignSubjectToInstructorAsync(instructorId, subjectId);
        if (!ok) return NotFound();
        return Ok();
    }

    [HttpDelete("{instructorId:guid}/subjects/{subjectId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnassignSubject(Guid instructorId, Guid subjectId)
    {
        var ok = await _service.RemoveSubjectFromInstructorAsync(instructorId, subjectId);
        if (!ok) return NotFound();
        return NoContent();
    }
}
