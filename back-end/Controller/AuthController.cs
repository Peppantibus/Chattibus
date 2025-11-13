using Chat.Models.Dto;
using Chat.Models.Entity;
using Chat.Routes;
using Chat.Services.AuthService;
using Chat.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Chat.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route(ApiRoutes.Auth.Register)]
        public async Task<IActionResult> Register([FromBody] RegisterDto body)
        {
            var mappedItem = SimpleMapper.Map<RegisterDto, User>(body);
            await _authService.AddUser(mappedItem);

            //rimappo in userDto per evitare di mostrare password in chiaro
            var mappedResult = SimpleMapper.Map<User, UserDto>(mappedItem);
            return Ok(mappedResult);
        }

        [HttpPost]
        [Route(ApiRoutes.Auth.Login)]
        public async Task<IActionResult> Login([FromBody] LoginDto body)
        {
            var response = await _authService.Login(body.Username, body.Password);

            return Ok(response);
        }

        [HttpPost]
        [Route(ApiRoutes.Auth.RefreshToken)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto body) => Ok(await _authService.RefreshToken(body.Token));
    }
}
