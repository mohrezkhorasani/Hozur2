using Application.DTO;
using AutoMapper;
using CoreMe.VM;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<NewUserViewModel, NewMeetingRequest>()
            .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
    }
}
