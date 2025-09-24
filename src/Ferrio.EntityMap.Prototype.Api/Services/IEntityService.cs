using System;
using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public interface IEntityService
{
    Task<Entity> CreateEntity(Guid environmentId, CreateEntity entity);

    Task CreateEntityMap(CreateEntityMap entityMap);

    Task<MappedEntities> CreateEntityPair(CreateMappedEntities createMappedEntities);
}
