using AmsApi.Interfaces;
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
    private readonly IInstructorService _InstructorService;
    private readonly IJwtHelper _jwtHelper;

    public InstructorsController(IInstructorService service, IJwtHelper jwtHelper)
    {
        _InstructorService = service;
        _jwtHelper = jwtHelper;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _InstructorService.GetAllAsync();
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create([FromBody] CreateInstructorDto dto)
    {
        var inst = await _InstructorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetOne), new { instructorId = inst.Id }, inst);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto payload)
    {
        var inst = await _InstructorService.GetByEmailAsync(payload.Username);
        if (inst == null || inst.Password != payload.Password)
            return Unauthorized();

        var token = _jwtHelper.GenerateToken(inst.Id, "Instructor");
        return Ok(new { token });
    }


    [HttpGet("{instructorId:guid}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> GetOne(Guid instructorId)
    {
        var inst = await _InstructorService.GetByIdAsync(instructorId);
        if (inst == null) return NotFound();
        return Ok(inst);
    }

    [HttpPatch("{instructorId:guid}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Update(Guid instructorId, [FromBody] UpdateInstructorDto dto)
    {
        var updated = await _InstructorService.UpdateAsync(instructorId, dto);
        if (updated == null) return NotFound();
        return NoContent();
    }

    [HttpDelete("{instructorId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid instructorId)
    {
        var ok = await _InstructorService.DeleteAsync(instructorId);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpPost("{instructorId}/image")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> UploadImage(Guid instructorId, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No image file uploaded" });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        try
        {
            await _InstructorService.UploadImageAsync(instructorId, ms.ToArray());

            var imageUrl = $"{Request.Scheme}://{Request.Host}//Instructors/{instructorId}/Instructor.png";


            return Ok(new
            {
                message = "Image uploaded successfully",
                imageUrl
            });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{instructorId:guid}/subjects")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> GetSubjects(Guid instructorId)
    {
        var list = await _InstructorService.GetSubjectsForInstructorAsync(instructorId);
        return Ok(list);
    }

    [HttpGet("{instructorId}/subjects/{subjectId}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> GetSubject(Guid instructorId, Guid subjectId)
    {
        var subj = await _InstructorService.GetSubjectForInstructorAsync(instructorId, subjectId);
        if (subj == null) return NotFound();
        return Ok(subj);
    }

    [HttpPut("{instructorId}/subjects/{subjectId}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> AssignSubject(Guid instructorId, Guid subjectId)
    {
        var ok = await _InstructorService.AssignSubjectToInstructorAsync(instructorId, subjectId);
        if (!ok) return NotFound();
        return Ok();
    }

    [HttpDelete("{instructorId:guid}/subjects/{subjectId:guid}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> UnassignSubject(Guid instructorId, Guid subjectId)
    {
        var ok = await _InstructorService.RemoveSubjectFromInstructorAsync(instructorId, subjectId);
        if (!ok) return NotFound();
        return NoContent();
    }
}
