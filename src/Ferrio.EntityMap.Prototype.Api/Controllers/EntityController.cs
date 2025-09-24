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

    [HttpPost("entitymaps")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> MapExistingEntities([FromBody] CreateEntityMapRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid request.");
        }

        await _entityService.CreateEntityMap(request.ToModel());
        return CreatedAtAction(nameof(MapExistingEntities), null);
    }

    [HttpPost("entitymaps/pairs")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEntityPair([FromBody] CreateMappedEntityPairRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid request.");
        }

        var entities = await _entityService.CreateEntityPair(request.ToModel());
        return CreatedAtAction(nameof(CreateEntityPair), entities);
    }
}

