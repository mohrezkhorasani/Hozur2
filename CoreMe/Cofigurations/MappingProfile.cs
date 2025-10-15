using Application.DTO;
using Application.Usecase;
using AutoMapper;
using CoreMe.VM;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<NewUserViewModel, NewMeetingRequest>()
            .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());

        CreateMap<UpdateMeetingViewModel, UpdateMeetingRequest>()
            .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());

        // Mapping برای NewUserMettingsViewModel
        CreateMap<NewUserMettingsViewModel, NewUserMettingsRequest>()
            .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());

        // Mapping برای NewTeacherViewModel
        CreateMap<NewTeacherViewModel, NewMeetingRequest>()
            .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());

        // Mapping برای NewMeetingViewModel
        CreateMap<NewMeetingViewModel, NewMeetingRequest>()
            .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<NewRoomViewModel, NewRoomRequest>()
            .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<NewUserMeetingLogViewModel, NewUserMeetingLogRequest>()
                 .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        
        CreateMap<NewUserMeetingLogFromModeratorViewModel, NewUserMeetingLogFromModeratorRequest>()
                 .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        

    }
}