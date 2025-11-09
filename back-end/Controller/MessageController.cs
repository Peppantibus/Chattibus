using Chat.Models.Dto;
using Chat.Services.MessagService;
using Chat.Utilities;
using Chat.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Chat.Routes;

namespace Chat.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }
        [HttpGet]
        [Route(ApiRoutes.Messages.All)]
        public async Task<List<MessageDto>> GetAllMessages() => await _messageService.GetAllMessages();
       
    }
}
