using System;
using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public interface IEntityService
{
    Task<Entity> CreateEntity(Guid tenantId, Guid environmentId, Entity entity);

    Task CreateEntityMap(Guid tenantId, CreateEntityMap entityMap);

    Task<MappedEntities> CreateEntityPair(Guid tenantId, CreateMappedEntities createMappedEntities);

    Task CreateEntitySettings(Guid tenantId, Guid environmentId, string entityId, Dictionary<string, string> settings);
}
