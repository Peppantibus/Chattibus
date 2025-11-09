using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class FriendController : ControllerBase
    {
        private readonly IFriendService _friendService;

        public FriendController(IFriendService friendservice) {
            _friendService = friendservice;
        }

        [HttpGet]
        [Route(ApiRoutes.Friends.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _friendService.GetAllFriends();
           
            return Ok(result);
        }

        [HttpDelete]
        [Route(ApiRoutes.Friends.Delete)]
        public async Task<IActionResult> DeleteFriend(int id) 
        {
            await _friendService.DeleteFriend(id);
            return Ok(new { message = "utente eliminato con successo" });

        }

    }
}
