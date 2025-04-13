namespace AmsApi.Helpers;

public class AttendeeImageUrlResolver : IValueResolver<Attendee, AttendeeDto, string?>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AttendeeImageUrlResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Resolve(Attendee source, AttendeeDto destination, string? destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.ImagePath)) return null;

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = $"{request?.Scheme}://{request?.Host}";
        return baseUrl + source.ImagePath;
    }
}
