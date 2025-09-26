using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public interface IEnvironmentService
{
    Task<Models.Environment> CreateEnvironment(Guid tenantId, Guid applicationId, CreateEnvironment environment);

    Task CreateEnvironmentCapabilityMap(Guid sourceEnvironmentId, Guid targetEnvironmentId, Dictionary<string, bool> capabilities);

    Task CreateEnvironmentSettings(Guid tenantId, Guid environmentId, Dictionary<string, string> settings);
}
