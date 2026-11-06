using ForwardingLetterPortal.API.Infrastructure;
using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.DTOs.Common;
using ForwardingLetterPortal.Interface.DTOs.Letters;
using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForwardingLetterPortal.API.Controllers
{
    /// <summary>Steps 8-11: generate, preview, print and list forwarding letters.</summary>
    [ApiController]
    [Route("api/letters")]
    [Authorize]
    public class LettersController(ILetterService letters) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<LetterDto>> Generate(GenerateLetterDto dto)
        {
            var letter = await letters.GenerateAsync(User.OfficerId(), dto);
            return CreatedAtAction(nameof(Get), new { id = letter.LetterId }, letter);
        }

        /// <summary>Record management. Officers see their own letters; Super Admin sees all.</summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<LetterListItemDto>>> List([FromQuery] LetterQueryDto query)
            => Ok(await letters.ListAsync(User.OfficerId(), User.IsSuperAdmin(), query));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LetterDto>> Get(int id)
            => Ok(await letters.GetAsync(User.OfficerId(), User.IsSuperAdmin(), id));

        /// <summary>PDF for on-screen preview (logged, does not count as a print).</summary>
        [HttpGet("{id:int}/preview")]
        public async Task<IActionResult> Preview(int id)
            => File(await letters.GetPdfAsync(User.OfficerId(), User.IsSuperAdmin(), id,
                PrintActions.Preview, HttpContext.ClientIp()), "application/pdf");

        /// <summary>PDF to send to the printer; increments the print count.</summary>
        [HttpGet("{id:int}/print")]
        public async Task<IActionResult> Print(int id)
            => File(await letters.GetPdfAsync(User.OfficerId(), User.IsSuperAdmin(), id,
                PrintActions.Print, HttpContext.ClientIp()), "application/pdf");
    }
}
