using ForwardingLetterPortal.Interface.DTOs.Catalog;
using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForwardingLetterPortal.API.Controllers
{
    /// <summary>Steps 3-4: retail services and their predefined purposes.</summary>
    [ApiController]
    [Route("api/services")]
    [Authorize]
    public class CatalogController(ICatalogService catalog) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<ServiceDto>>> Services() => Ok(await catalog.GetServicesAsync());

        [HttpGet("{serviceId:int}/purposes")]
        public async Task<ActionResult<List<PurposeDto>>> Purposes(int serviceId)
            => Ok(await catalog.GetPurposesAsync(serviceId));
    }
}
