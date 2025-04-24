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
        if (string.IsNullOrEmpty(source.ImagePath))
            return null;

        var request = _httpContextAccessor.HttpContext?.Request;

        // ✅ دي بتحذف أي /api أو /attendees لأنها بتعتمد فقط على Scheme + Host
        var baseUrl = $"{request?.Scheme}://{request?.Host.Value}";

        // ✅ تأكد إن المسار يبدأ بـ /
        var imagePath = source.ImagePath.StartsWith("/") ? source.ImagePath : "/" + source.ImagePath;

        return baseUrl + imagePath;
    }
}
