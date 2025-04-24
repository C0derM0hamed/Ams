using AmsApi.DTOs;
using AmsApi.Models;
using AutoMapper;

namespace AmsApi.Helpers
{
    public class InstructorImageUrlResolver : IValueResolver<Instructor, InstructorDto, string?>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InstructorImageUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? Resolve(Instructor source, InstructorDto destination, string? destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.ImagePath)) return null;

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";
            return baseUrl + source.ImagePath;
        }
    }
}
