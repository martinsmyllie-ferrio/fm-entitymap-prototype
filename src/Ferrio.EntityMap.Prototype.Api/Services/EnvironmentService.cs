using System;
using Ferrio.EntityMap.Prototype.Api.Persistence;
using Ferrio.EntityMap.Prototype.Api.Services.Models;
using Neo4j.Driver;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public class EnvironmentService(IEntityMapStorage storage, ILogger<EnvironmentService> logger) : IEnvironmentService
{
    private readonly IEntityMapStorage _storage = storage;
    private readonly ILogger<EnvironmentService> _logger = logger;

    public async Task<Models.Environment> CreateEnvironment(Guid applicationId, CreateEnvironment environment)
    {
        _logger.LogInformation("Creating environment {EnvironmentName} for application {ApplicationId}", environment.Name, applicationId);

        var env = new Models.Environment
        {
            Id = Guid.NewGuid(),
            Name = environment.Name
        };

        await _storage.CreateApplicationEnvironment(applicationId, env);

        return env;
    }
}
