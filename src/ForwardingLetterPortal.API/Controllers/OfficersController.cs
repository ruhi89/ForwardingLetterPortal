using ForwardingLetterPortal.API.Infrastructure;
using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.DTOs.Officers;
using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForwardingLetterPortal.API.Controllers
{
    /// <summary>Step 2: Super Admin creates, manages and maintains officer accounts.</summary>
    [ApiController]
    [Route("api/officers")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class OfficersController(IOfficerService officers) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<OfficerDto>>> List() => Ok(await officers.ListAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OfficerDto>> Get(int id) => Ok(await officers.GetAsync(id));

        [HttpPost]
        public async Task<ActionResult<OfficerDto>> Create(CreateOfficerDto dto)
        {
            var created = await officers.CreateAsync(User.OfficerId(), dto);
            return CreatedAtAction(nameof(Get), new { id = created.OfficerId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<OfficerDto>> Update(int id, UpdateOfficerDto dto)
            => Ok(await officers.UpdateAsync(User.OfficerId(), id, dto));

        [HttpPost("{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, ResetPasswordDto dto)
        {
            await officers.ResetPasswordAsync(id, dto);
            return NoContent();
        }

        [HttpPost("{id:int}/unlock")]
        public async Task<IActionResult> Unlock(int id)
        {
            await officers.UnlockAsync(id);
            return NoContent();
        }
    }
}
