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

    public async Task<MappedEntities> CreateEntityPair(CreateMappedEntities createMappedEntities)
    {
        _logger.LogInformation("Creating entity pair with mapping");

        var mappedEntities = new MappedEntities
        {
            SourceEnvironmentId = createMappedEntities.SourceEnvironmentId,
            TargetEnvironmentId = createMappedEntities.TargetEnvironmentId,
            SourceEntity = new()
            {
                Id = Guid.NewGuid(),
                Name = createMappedEntities.SourceEntity.Name,
                ReferenceId = createMappedEntities.SourceEntity.ReferenceId,
                ParentReferenceId = createMappedEntities.SourceEntity.ParentReferenceId,
                EntityType = createMappedEntities.SourceEntity.EntityType
            },
            TargetEntity = new()
            {
                Id = Guid.NewGuid(),
                Name = createMappedEntities.TargetEntity.Name,
                ReferenceId = createMappedEntities.TargetEntity.ReferenceId,
                ParentReferenceId = createMappedEntities.TargetEntity.ParentReferenceId,
                EntityType = createMappedEntities.TargetEntity.EntityType
            }
        };

        await _storage.CreateEntityPairWithMap(mappedEntities);

        return mappedEntities;
    }
}
