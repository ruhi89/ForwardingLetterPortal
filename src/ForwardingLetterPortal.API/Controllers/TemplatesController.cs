using ForwardingLetterPortal.API.Infrastructure;
using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.DTOs.Templates;
using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForwardingLetterPortal.API.Controllers
{
    /// <summary>Super Admin manages letter templates and which one is active per service/purpose/letter type.</summary>
    [ApiController]
    [Route("api/templates")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class TemplatesController(ITemplateService templates) : ControllerBase
    {
        [HttpGet("placeholders")]
        public ActionResult<List<PlaceholderDto>> Placeholders() => Ok(templates.GetPlaceholders());

        [HttpGet]
        public async Task<ActionResult<List<TemplateDto>>> List(
            int? serviceId, int? purposeId, string? letterType, bool? isActive)
            => Ok(await templates.ListAsync(serviceId, purposeId, letterType, isActive));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TemplateDto>> Get(int id) => Ok(await templates.GetAsync(id));

        [HttpPost]
        public async Task<ActionResult<TemplateDto>> Create(SaveTemplateDto dto)
        {
            var created = await templates.CreateAsync(User.OfficerId(), dto);
            return CreatedAtAction(nameof(Get), new { id = created.TemplateId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TemplateDto>> Update(int id, SaveTemplateDto dto)
            => Ok(await templates.UpdateAsync(User.OfficerId(), id, dto));

        /// <summary>Activates this template and deactivates the previously active one for the same key.</summary>
        [HttpPost("{id:int}/activate")]
        public async Task<ActionResult<TemplateDto>> Activate(int id)
            => Ok(await templates.SetActiveAsync(User.OfficerId(), id, true));

        [HttpPost("{id:int}/deactivate")]
        public async Task<ActionResult<TemplateDto>> Deactivate(int id)
            => Ok(await templates.SetActiveAsync(User.OfficerId(), id, false));
    }
}
