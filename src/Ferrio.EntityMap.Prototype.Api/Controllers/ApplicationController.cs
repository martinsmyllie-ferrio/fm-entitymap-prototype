using Ferrio.EntityMap.Prototype.Api.Contracts;
using Ferrio.EntityMap.Prototype.Api.Services;
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

    [HttpPost("tenants")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid tenant data.");
        }

        var tenant = await _applicationService.CreateTenant(request.ToModel());
        return CreatedAtAction(nameof(CreateTenant), tenant);
    }

    [HttpPost("domains")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDomain([FromHeader(Name = "X-Tenant-ID")] Guid tenantId, [FromBody] CreateDomainRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid domain data.");
        }

        var domain = await _applicationService.CreateDomain(tenantId, request.ToModel());
        return CreatedAtAction(nameof(CreateDomain), domain);
    }

    [HttpPost("domains/{domainId:guid}/apps")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateApplication([FromHeader(Name = "X-Tenant-ID")] Guid tenantId, Guid domainId, [FromBody] CreateApplicationRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid application data.");
        }

        var application = await _applicationService.CreateApplication(tenantId, domainId, request.ToModel());
        return CreatedAtAction(nameof(CreateApplication), application);
    }

    [HttpPost("apps/{appId:guid}/environments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateApplicationEnvironment([FromHeader(Name = "X-Tenant-ID")] Guid tenantId, Guid appId, [FromBody] CreateEnvironmentRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid environment data.");
        }

        var environment = await _environmentService.CreateEnvironment(tenantId, appId, request.ToModel());
        return CreatedAtAction(nameof(CreateApplicationEnvironment), environment);
    }

    [HttpPost("environments/{envId:guid}/capabilities")]
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

    [HttpPost("environments/{environmentId:guid}/settings")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEnvironmentSettings([FromHeader(Name = "X-Tenant-ID")] Guid tenantId, Guid environmentId, [FromBody] CreateSettingsRequest[] request)
    {
        if (request == null)
        {
            return BadRequest("Invalid environment data.");
        }

        await _environmentService.CreateEnvironmentSettings(tenantId, environmentId, request.ToModel());
        return CreatedAtAction(nameof(CreateEnvironmentSettings), null);
    }
}
