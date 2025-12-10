using Microsoft.AspNetCore.Mvc;

namespace NajmBridge.Controllers
{
    public class MemberController : Controller
    {




        [HttpGet("/Members/{idMember}")]
        public async Task<IActionResult> GetMemberByID([FromRoute] string meetid, [FromRoute] string idMember)
        {
            throw new NotImplementedException();
            //info of user
        }

        [HttpGet("/Members/{idMember}/GetMeetings")]
        public async Task<IActionResult> GetMemberMeeting([FromRoute] string meetid, [FromRoute] string idMember)
        {
            throw new NotImplementedException();
            //info of user
        }
        [HttpGet("/Members/{idMember}/GetCertificates")]
        public async Task<IActionResult> GetMemberCertificates([FromRoute] string meetid, [FromRoute] string idMember)
        {
            throw new NotImplementedException();
            //info of user
        }
    }
}
