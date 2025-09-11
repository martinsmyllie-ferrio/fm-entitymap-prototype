using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public interface IEnvironmentService
{
    Task<Models.Environment> CreateEnvironment(Guid applicationId, CreateEnvironment environment);

    // void CreateEnvironment(string applicationType, string name);
}
