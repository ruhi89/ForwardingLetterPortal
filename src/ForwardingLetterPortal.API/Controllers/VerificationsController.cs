using ForwardingLetterPortal.API.Infrastructure;
using ForwardingLetterPortal.Interface.DTOs.Verification;
using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForwardingLetterPortal.API.Controllers
{
    /// <summary>Steps 5-7: customer information, OTP verification and account retrieval.</summary>
    [ApiController]
    [Route("api/verifications")]
    [Authorize]
    public class VerificationsController(IVerificationService verifications) : ControllerBase
    {
        /// <summary>Checks account number + mobile/e-mail against bank records and sends an OTP.</summary>
        [HttpPost]
        public async Task<ActionResult<StartVerificationResponseDto>> Start(StartVerificationDto dto)
        {
            var result = await verifications.StartAsync(User.OfficerId(), dto, HttpContext.ClientIp());
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Confirms the OTP; on success returns the customer's account information.</summary>
        [HttpPost("{id:int}/confirm")]
        public async Task<ActionResult<ConfirmOtpResponseDto>> Confirm(int id, ConfirmOtpDto dto)
        {
            var result = await verifications.ConfirmAsync(User.OfficerId(), id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:int}/customer")]
        public async Task<ActionResult<CustomerInfoDto>> Customer(int id)
            => Ok(await verifications.GetVerifiedCustomerAsync(User.OfficerId(), id));
    }
}
