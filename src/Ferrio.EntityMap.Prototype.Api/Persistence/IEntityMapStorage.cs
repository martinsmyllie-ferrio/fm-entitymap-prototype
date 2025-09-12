using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Persistence;

public interface IEntityMapStorage
{
    Task TestConnectionAsync();

    Task CreateApplication(Application application);

    Task CreateApplicationEnvironment(Guid appId, Services.Models.Environment environment);

    Task CreateEntity(Guid environmentId, Services.Models.Entity entity);

    Task CreateEntityMap(CreateEntityMap createEntityMap);
}
