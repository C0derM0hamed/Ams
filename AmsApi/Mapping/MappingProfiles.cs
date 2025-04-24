namespace AmsApi.Mapping
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Attendee, AttendeeDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom<AttendeeImageUrlResolver>());
            
            CreateMap<Instructor, InstructorDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom<InstructorImageUrlResolver>()); // نضيف ده كمان
           
            CreateMap<CreateSubjectDto, Subject>();
            CreateMap<UpdateSubjectDto, Subject>();
            CreateMap<Subject, SubjectDto>().ReverseMap();

            CreateMap<Attendance, AttendanceDto>().ReverseMap();

            CreateMap<CreateSubjectDateDto, SubjectDate>();

        }
    }

}
