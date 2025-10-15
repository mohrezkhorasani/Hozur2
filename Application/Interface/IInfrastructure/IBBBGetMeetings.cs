using Application.DTO;
using Application.DTO.Models;
using Application.Interface.IUsecase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IInfrastructure
{
    public interface IBBBGetMeetings
    {
        public Task<List<MiniRoomJSON>> Request(GetLivesRequest req);
        public Task<MeetJSON> CreateMeetingAsync(string meetingID, string meetingName);
        public Task<string> GetJoinUrlAsync(string meetingID, string fullName,bool asmoderator, string moderatorPassowrd);
    }
}
