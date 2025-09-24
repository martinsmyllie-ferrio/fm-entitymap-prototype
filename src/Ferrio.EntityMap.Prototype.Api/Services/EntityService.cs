using System;
using Ferrio.EntityMap.Prototype.Api.Persistence;
using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public class EntityService(IEntityMapStorage storage, ILogger<EntityService> logger) : IEntityService
{
    private readonly IEntityMapStorage _storage = storage;
    private readonly ILogger<EntityService> _logger = logger;

    public async Task<Entity> CreateEntity(Guid tenantId, Guid environmentId, Entity entity)
    {
        _logger.LogInformation("Creating {EntityType} entity {EntityName} for environment {EnvironmentId}", entity.EntityType, entity.Name, environmentId);

        await _storage.CreateEntity(tenantId, environmentId, entity);

        return entity;
    }

    public Task CreateEntityMap(CreateEntityMap entityMap)
    {
        _logger.LogInformation("Creating entity map");

        return _storage.CreateEntityMap(entityMap);
    }

    public async Task<MappedEntities> CreateEntityPair(CreateMappedEntities createMappedEntities)
    {
        _logger.LogInformation("Creating entity pair with mapping");

        var mappedEntities = new MappedEntities
        {
            SourceEnvironmentId = createMappedEntities.SourceEnvironmentId,
            TargetEnvironmentId = createMappedEntities.TargetEnvironmentId,
            SourceEntity = new()
            {
                Name = createMappedEntities.SourceEntity.Name,
                Id = createMappedEntities.SourceEntity.Id,
                ParentId = createMappedEntities.SourceEntity.ParentId,
                EntityType = createMappedEntities.SourceEntity.EntityType
            },
            TargetEntity = new()
            {
                Name = createMappedEntities.TargetEntity.Name,
                Id = createMappedEntities.TargetEntity.Id,
                ParentId = createMappedEntities.TargetEntity.ParentId,
                EntityType = createMappedEntities.TargetEntity.EntityType
            }
        };

        await _storage.CreateEntityPairWithMap(mappedEntities);

        return mappedEntities;
    }
}
