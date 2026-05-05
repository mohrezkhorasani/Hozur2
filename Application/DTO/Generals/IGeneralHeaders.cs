using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Generals
{
    public class GeneralHeaders : IGeneralHeaders
    {
        public string Agent { get; set; }
        public string? JWTMeetingToken { get; set; }
        public string? JWTMeetingUserToken { get; set; }
        public string? JWTUserToken { get; set; }
        public string RequestedPage { get; set; }
        public string RequestedIP { get; set; }
        public string RequestedDevice { get; set; }
        public Guid IdentificationCode { get; set; }
        public string Referer { get; set; }
        public string RefererGoogle { get; set; }
        public string? MeetIdentificationCode { get; set; }
        public string? RoomID { get; set; }
        public string? Role { get; set; }
        public UserTBL UserTBL { get; set; }
    }
    public interface IGeneralHeaders
    {
        public string Agent { get; set; }
        public string? JWTMeetingToken { get; set; }
        public string? JWTUserToken { get; set; }
        public string RequestedPage { get; set; }
        public string RequestedIP { get; set; }
        public string? JWTMeetingUserToken { get; set; }
        public string RequestedDevice { get; set; }
        public Guid IdentificationCode { get; set; }
        public string? MeetIdentificationCode { get; set; }
        public string? RoomID { get; set; }
        public string Referer { get; set; }
        public string RefererGoogle { get; set; }
        public string? Role { get; set; }
        public UserTBL UserTBL { get; set; }
    }
}
