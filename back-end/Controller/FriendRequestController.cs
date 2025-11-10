using Chat.Enum;
using Chat.Models.Dto;
using Chat.Models.Entity;
using Chat.Routes;
using Chat.Services.FriendService;
using Chat.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FriendRequestController : ControllerBase
    {
        private readonly IFriendService _friendService;

        public FriendRequestController(IFriendService friendservice)
        {
            _friendService = friendservice;
        }

        [HttpGet]
        [Route(ApiRoutes.FriendRequests.GetAll)]
        public async Task<IActionResult> GetFriend([FromQuery] FriendRequestType type) => Ok(await _friendService.GetFriendRequests(type));

        [HttpPost]
        [Route(ApiRoutes.FriendRequests.Send)]
        public async Task<IActionResult> SendRequest(string username)
        {
            await _friendService.Send(username);
            return Ok(new { message = $"Richiesta inviata all'utente {username} con successo" });
        }

        [HttpPut]
        [Route(ApiRoutes.FriendRequests.Accept)]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            await _friendService.Accept(id);
            return Ok(new { message = "utente ha accettato correttamente la richiesta" });
        }

        [HttpDelete]
        [Route(ApiRoutes.FriendRequests.Delete)]
        public async Task<IActionResult> DeleteRequest(int id)
        {

            await _friendService.DeleteFriendRequest(id);
            return Ok(new { message = "utente eliminato con successo" });
        }
    }
}
