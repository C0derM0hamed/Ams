namespace AmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectDatesController : ControllerBase
{
    private readonly ISubjectDateService _subjectDateService;

    public SubjectDatesController(ISubjectDateService subjectDateService)
    {
        _subjectDateService = subjectDateService;
    }

    // GET /api/subjectdates/subject/{subjectId}
    [HttpGet("subject/{subjectId}")]
    public async Task<IActionResult> GetBySubjectId(int subjectId)
    {
        var dates = await _subjectDateService.GetDatesBySubjectId(subjectId);
        return Ok(dates);
    }

    // POST /api/subjectdates
    [HttpPost]
    public async Task<IActionResult> Create(CreateSubjectDateDto dto)
    {
        var result = await _subjectDateService.AddAsync(dto);
        return CreatedAtAction(nameof(GetBySubjectId), new { subjectId = dto.SubjectId }, result);
    }

}
