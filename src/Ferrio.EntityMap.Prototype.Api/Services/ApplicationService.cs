using System;
using Ferrio.EntityMap.Prototype.Api.Persistence;
using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public class ApplicationService(IEntityMapStorage storage, ILogger<ApplicationService> logger) : IApplicationService
{
    private readonly IEntityMapStorage _storage = storage;
    private readonly ILogger<ApplicationService> _logger = logger;

    public async Task<Application> CreateApplication(CreateApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        await _storage.TestConnectionAsync();

        var entityTypes = application.EntityDefinitions.Select(ed => ed.EntityType).ToHashSet();

        foreach (var entityDefinition in application.EntityDefinitions)
        {
            if (entityDefinition.ParentEntityType != null && !entityTypes.Contains(entityDefinition.ParentEntityType))
            {
                throw new ArgumentException($"Parent entity type '{entityDefinition.ParentEntityType}' for entity '{entityDefinition.Name}' does not exist in the provided entity definitions.");
            }
        }

        _logger.LogInformation("Creating application {ApplicationName} of type {ApplicationType} with {EntityCount} entity type definitions.",
            application.Name, application.ApplicationType, application.EntityDefinitions.Length);

        var app = new Application
        {
            Id = Guid.NewGuid(),
            Name = application.Name,
            ApplicationType = application.ApplicationType,
            EntityDefinitions = Array.ConvertAll(application.EntityDefinitions, ed => new EntityDefinition
            {
                Id = Guid.NewGuid(),
                Name = ed.Name,
                EntityType = ed.EntityType,
                ParentEntityType = ed.ParentEntityType
            })
        };

        await _storage.CreateApplication(app);

        return app;
    }

    public Application[] GetApplications()
    {
        return [];
    } 
}
