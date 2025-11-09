using Chat.Models.Dto;
using Chat.Models.Entity;
using Chat.Services.UserSerice;
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
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService, IOptions<JwtSettings> jwtSettings)
        {
            _authService = authService;
            _jwtSettings = jwtSettings;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto body)
        {
            var mappedItem = SimpleMapper.Map<RegisterDto, User>(body);
            await _authService.AddUser(mappedItem);

            //rimappo in userDto per evitare di mostrare password in chiaro
            var mappedResult = SimpleMapper.Map<User, UserDto>(mappedItem);
            return Ok(mappedResult);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto body)
        {
            string jwt = await _authService.Login(body.Username, body.Password);
            return Ok(new
            {
                token = jwt,
                expiresIn = _jwtSettings.Value.TokenLifetimeHours * 3600,
                user = new { username = body.Username }
            });
        }

    }
}
