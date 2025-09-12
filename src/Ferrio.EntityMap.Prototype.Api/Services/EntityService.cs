using System;
using Ferrio.EntityMap.Prototype.Api.Persistence;
using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public class EntityService(IEntityMapStorage storage, ILogger<EntityService> logger) : IEntityService
{
    private readonly IEntityMapStorage _storage = storage;
    private readonly ILogger<EntityService> _logger = logger;

    public async Task<Entity> CreateEntity(Guid environmentId, CreateEntity entity)
    {
        _logger.LogInformation("Creating {EntityType} entity {EntityName} for environment {EnvironmentId}", entity.EntityType, entity.Name, environmentId);

        var ent = new Entity
        {
            Id = Guid.NewGuid(),
            ReferenceId = entity.ReferenceId,
            ParentReferenceId = entity.ParentReferenceId,
            Name = entity.Name,
            EntityType = entity.EntityType
        };

        await _storage.CreateEntity(environmentId, ent);

        return ent;
    }

    public Task CreateEntityMap(CreateEntityMap entityMap)
    {
        _logger.LogInformation("Creating entity map");

        return _storage.CreateEntityMap(entityMap);
    }
}
