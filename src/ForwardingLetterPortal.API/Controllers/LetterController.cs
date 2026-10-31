using ForwardingLetterPortal.Interface.DTOs.Letter;
using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ForwardingLetterPortal.API.Controllers
{
    [ApiController]
    [Route("api/letters")]
    [Authorize]
    public class LetterController(ILetterService letterService) : ControllerBase
    {
        private int CustomerId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create(CreateLetterRequestDto dto)
        {
            var result = await letterService.CreateRequestAsync(CustomerId, dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyRequests()
        {
            var result = await letterService.GetMyRequestsAsync(CustomerId);
            return Ok(result);
        }

        [HttpGet("{requestId}/generate-pdf")]
        public async Task<IActionResult> GeneratePdf(int requestId)
        {
            try
            {
                var pdf = await letterService.GeneratePdfAsync(requestId, CustomerId);
                return File(pdf, "application/pdf", $"forwarding-letter-{requestId}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
