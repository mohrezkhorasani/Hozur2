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

        CreateMap<GenerateCertificateViewModel, GenerateCertificateRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<GuarantCertificateViewModel, GuarantCertificateRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<EndTheMeetingViewModel, EndTheMeetingRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<StartMeetingViewModel, StartNewMeetRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<NewQuestionViewModel, NewQuestionRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<SignInUserPanelViewModel, SignInUserPanelRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<LoginUserPanelViewModel, LoginUserPanelRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<VerifyTokenViewModel, VerifyTokenRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<AdminCreateRoomVM, AdminCreateRoomRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<AdminChangeRoleVM, AdminChangeRoleRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<NewPlanRoomViewModel, NewPlanRoomRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<EditSlideShowVM, EditSlideShowRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<InsertSlideShowVM, InsertSlideShowRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<ChangeAvatarOfRoomVM, ChangeAvatarOfRoomRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());
        CreateMap<EndTheMeetingWithAPIViewModel, EndTheMeetingWithAPIRequest>()
          .ForMember(dest => dest.GeneralHeaders, opt => opt.Ignore());

    }
}