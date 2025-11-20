using Chat.Models.Dto;
using Chat.Models.Dto.Auth;
using Chat.Models.Entity;
using Chat.Routes;
using Chat.Services.Auth;
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
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;
        public AuthController(IAuthService authService, IConfiguration config, ITokenService tokenService)
        {
            _authService = authService;
            _config = config;
            _tokenService = tokenService;
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
            var serviceResponse = await _authService.Login(body.Username, body.Password);

            Response.Cookies.Append(
                "refreshToken",
                serviceResponse.NewRefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,      // o false se http
                    SameSite = SameSiteMode.None,
                    Expires = serviceResponse.RefreshTokenExpiresAt
                }
            );

            var response = new AuthResponseDto
            {
                AccessExpiresIn = serviceResponse.AccessToken.ExpiresInSeconds,
                AccessToken = serviceResponse.AccessToken.Token,
                User = new UserDto
                {
                    Id = serviceResponse.User.Id,
                    Username = serviceResponse.User.Username,
                    Name = serviceResponse.User.Name,
                    LastName = serviceResponse.User.LastName,
                }
            };

            return Ok(response);
        }

        [HttpPost]
        [Route(ApiRoutes.Auth.RefreshToken)]
        public async Task<IActionResult> RefreshToken()
        {
            // 1. Leggi cookie HttpOnly
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Nessun refresh token");

            // 2. Chiama il service per ruotare i token
            var result = await _tokenService.RefreshToken(refreshToken);

            if (result == null)
                return Unauthorized("refresh token non valido");

            
            Response.Cookies.Append(
                "refreshToken",
                result.NewRefreshToken,     // il nuovo token generato dal service
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.None,
                    Expires = result.RefreshTokenExpiresAt
                }
            );

            // 4. Ritorna solo access token e user
            return Ok(new AuthResponseDto
            {
                AccessToken = result.AccessToken.Token,
                AccessExpiresIn = result.AccessToken.ExpiresInSeconds,
                User = result.User
            });
        }


        [HttpGet]
        [Route(ApiRoutes.Auth.VerifyMail)]
        public async Task<IActionResult> VerifyMail([FromQuery] Guid token)
        {
            var result = await _authService.VerifyMail(token);

            if (!result)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.Auth.ValidatePassword)]
        public async Task<IActionResult> ResetPasswordRedirect([FromQuery] Guid token)
        {
            var result = await _authService.ResetPasswordRedirect(token);

            return Ok(result);
        }


        [HttpPost]
        [Route(ApiRoutes.Auth.RecoveryPassword)]
        public async Task<IActionResult> RecoveryPassword([FromBody] RecoveryPasswordDto body)
        {
            var response = await _authService.RecoveryPassword(body.Email);

            return Ok(response);
        }

        [HttpPut]
        [Route(ApiRoutes.Auth.ResetPassword)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto body)
        {
            var result = await _authService.ResetPassword(body);

            return Ok(result);
        }
    }
}
