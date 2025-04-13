namespace AmsApi.Helpers;

public class InstructorImageUrlResolver : IValueResolver<Instructor, InstructorDto, string?>
{
    private readonly IHttpContextAccessor _http;

    public InstructorImageUrlResolver(IHttpContextAccessor http)
    {
        _http = http;
    }

    public string? Resolve(Instructor source, InstructorDto destination, string? destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.ImagePath)) return null;

        var request = _http.HttpContext?.Request;
        return $"{request?.Scheme}://{request?.Host}{source.ImagePath}";
    }
}
