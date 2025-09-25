using Ferrio.EntityMap.Prototype.Api.Contracts;
using Ferrio.EntityMap.Prototype.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ferrio.EntityMap.Prototype.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EntityController(IEntityService entityService, ILogger<EntityController> logger) : ControllerBase
{
    private readonly ILogger<EntityController> _logger = logger;
    private readonly IEntityService _entityService = entityService;

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy" });
    }

    [HttpPost("environments/{environmentId:guid}/entities")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEntity([FromHeader(Name = "X-Tenant-ID")] Guid tenantId, Guid environmentId, [FromBody] CreateEntityRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid request.");
        }

        var entity = await _entityService.CreateEntity(tenantId, environmentId, request.ToModel());
        return CreatedAtAction(nameof(CreateEntity), entity);
    }

    [HttpPost("environments/{environmentId:guid}/entities/{entityType}/{entityId}/settings")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEntitySettings([FromHeader(Name = "X-Tenant-ID")] Guid tenantId, Guid environmentId, string entityType, string entityId, [FromBody] CreateSettingsRequest[] request)
    {
        if (request == null)
        {
            return BadRequest("Invalid request.");
        }

        await _entityService.CreateEntitySettings(tenantId, environmentId, entityType, entityId, request.ToModel());
        return CreatedAtAction(nameof(CreateEntitySettings), null);
    }

    [HttpPost("mappings")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> MapExistingEntities([FromHeader(Name = "X-Tenant-ID")] Guid tenantId, [FromBody] CreateEntityMapRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid request.");
        }

        await _entityService.CreateEntityMap(tenantId, request.ToModel());
        return CreatedAtAction(nameof(MapExistingEntities), null);
    }

    [HttpPost("mappings/pairs")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEntityPair([FromHeader(Name = "X-Tenant-ID")] Guid tenantId, [FromBody] CreateMappedEntityPairRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid request.");
        }

        var entities = await _entityService.CreateEntityPair(tenantId, request.ToModel());
        return CreatedAtAction(nameof(CreateEntityPair), entities);
    }

    [HttpGet("environments/{environmentId:guid}/entities/{entityType}/{entityId}/settings/{settingName}")]
    public async Task<IActionResult> GetSetting([FromHeader(Name = "X-Tenant-ID")] Guid tenantId, Guid environmentId, string entityType, string entityId, string settingName)
    {
        var setting = await _entityService.GetEntitySetting(tenantId, environmentId, entityType, entityId, settingName);
        if (setting == null)
        {
            return NotFound();
        }

        return Ok(setting);
    }

    [HttpGet("environments/{environmentId:guid}/entities/{entityType}/{entityId}/mappings")]
    public async Task<IActionResult> GetMappedEntities([FromHeader(Name = "X-Tenant-ID")] Guid tenantId, Guid environmentId, string entityType, string entityId)
    {
        var entities = await _entityService.GetMappedEntities(tenantId, environmentId, entityType, entityId);

        return Ok(entities);
    }
}

