using Chat.Models.Dto;
using Chat.Models.Dto.Auth;
using Chat.Models.Entity;
using Chat.Routes;
using Chat.Services.AuthService;
using Chat.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;
        public AuthController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
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

        [HttpGet]
        [Route(ApiRoutes.Auth.VerifyMail)]
        public async Task<IActionResult> VerifyMail([FromQuery] Guid token)
        {
            var result = await _authService.VerifyMail(token);

            if (!result)
                return Redirect(_config["AppUrls:FrontEnd"] + "/verify?failed=true");

            return Redirect(_config["AppUrls:FrontEnd"] + "");
        }

        [HttpGet]
        [Route(ApiRoutes.Auth.ResetPassword)]
        public async Task<IActionResult> ResetPasswordRedirect([FromQuery] Guid token)
        {
            var result = await _authService.ResetPasswordRedirect(token);

            if (!result)
                return Redirect(_config["AppUrls:FrontEnd"] + $"/reset-password?token={token}");

            return Redirect(_config["AppUrls:FrontEnd"] + "/reset-password");
        }


        [HttpPost]
        [Route(ApiRoutes.Auth.RecoveryPassword)]
        public async Task<IActionResult> RecoveryPassword([FromBody] RecoveryPasswordDto body)
        {
            var response = await _authService.RecoveryPassword(body.Email);

            return Ok(new { Messages = response });
        }

        [HttpPut]
        [Route(ApiRoutes.Auth.ResetPassword)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto body)
        {
            var result = await _authService.ResetPassword(body);

            return Ok(new {Success = result});
        }
    }
}
