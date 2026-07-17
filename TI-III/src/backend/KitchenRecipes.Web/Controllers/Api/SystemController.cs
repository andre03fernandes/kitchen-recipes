using KitchenRecipes.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace KitchenRecipes.Web.Controllers.Api;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("health")]
    public async Task<IActionResult> Health(
        [FromServices] SystemStatusService systemStatusService,
        CancellationToken cancellationToken)
    {
        var status = await systemStatusService.GetStatusAsync(cancellationToken);
        return Ok(status);
    }
}
