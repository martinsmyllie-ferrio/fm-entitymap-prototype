using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Persistence;

public interface IEntityMapStorage
{
    Task TestConnectionAsync();

    Task CreateTenant(Tenant tenant);

    Task CreateDomain(Guid tenantId, Domain domain);

    Task CreateApplication(Guid tenantId, Guid domainId, Application application);

    Task CreateApplicationEnvironment(Guid tenantId, Guid applicationId, Services.Models.Environment environment);

    Task CreateEnvironmentCapabilityMap(Guid sourceEnvironmentId, Guid targetEnvironmentId, Dictionary<string, bool> capabilities);

    Task CreateEntity(Guid tenantId, Guid environmentId, Services.Models.Entity entity);

    Task CreateEntityMap(Guid tenantId, CreateEntityMap createEntityMap);

    Task CreateEntityPairWithMap(Guid tenantId, MappedEntities mappedEntities);

    Task CreateEnvironmentSettings(Guid tenantId, Guid environmentId, Dictionary<string, string> settings);

    Task CreateEntitySettings(Guid tenantId, Guid environmentId, string entityType, string entityId, Dictionary<string, string> settings);

    Task<string> GetEntitySetting(Guid tenantId, Guid environmentId, string entityType, string entityId, string settingName);
}
