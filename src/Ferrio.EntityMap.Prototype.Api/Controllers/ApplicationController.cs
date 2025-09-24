using Ferrio.EntityMap.Prototype.Api.Contracts;
using Ferrio.EntityMap.Prototype.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferrio.EntityMap.Prototype.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationController(IApplicationService applicationService, IEnvironmentService environmentService, ILogger<ApplicationController> logger) : ControllerBase
{
    private readonly ILogger<ApplicationController> _logger = logger;
    private readonly IApplicationService _applicationService = applicationService;
    private readonly IEnvironmentService _environmentService = environmentService;

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy" });
    }

    [HttpPost("apps")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid application data.");
        }

        var application = await _applicationService.CreateApplication(request.ToModel());
        return CreatedAtAction(nameof(CreateApplication), application);
    }

    [HttpPost("apps/{appId}/environments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateApplicationEnvironment(Guid appId, [FromBody] CreateEnvironmentRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid environment data.");
        }

        var environment = await _environmentService.CreateEnvironment(appId, request.ToModel());
        return CreatedAtAction(nameof(CreateApplicationEnvironment), environment);
    }

    [HttpPost("environments/{envId}/capabilities")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> MapEnvironmentCapabilities(Guid envId, [FromBody] EnvironmentPublishingCapabilities request)
    {
        if (request == null)
        {
            return BadRequest("Invalid environment data.");
        }

        await _environmentService.CreateEnvironmentCapabilityMap(envId, request.TargetEnvironmentId, request.PublishCapabilities);
        return CreatedAtAction(nameof(MapEnvironmentCapabilities), null);
    }
}
