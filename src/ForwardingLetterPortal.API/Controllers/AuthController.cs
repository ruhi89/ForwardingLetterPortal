using ForwardingLetterPortal.API.Infrastructure;
using ForwardingLetterPortal.Interface.DTOs.Auth;
using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForwardingLetterPortal.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService auth) : ControllerBase
    {
        /// <summary>Step 1: officer login (employee ID or e-mail + password).</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
            => Ok(await auth.LoginAsync(dto, HttpContext.ClientIp()));

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            await auth.ChangePasswordAsync(User.OfficerId(), dto);
            return NoContent();
        }
    }
}
