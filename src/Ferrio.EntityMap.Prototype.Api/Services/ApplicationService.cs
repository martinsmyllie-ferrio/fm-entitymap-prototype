using System;
using Ferrio.EntityMap.Prototype.Api.Persistence;
using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public class ApplicationService(IEntityMapStorage storage, ILogger<ApplicationService> logger) : IApplicationService
{
    private readonly IEntityMapStorage _storage = storage;
    private readonly ILogger<ApplicationService> _logger = logger;

    public async Task<Tenant> CreateTenant(Tenant tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);
    
        _logger.LogInformation("Creating Tenant {TenantId} with name {TenantName}", tenant.TenantId, tenant.Name);

        await _storage.CreateTenant(tenant);

        return tenant;
    }

    public async Task<Domain> CreateDomain(Guid tenantId, CreateDomain domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        var entityTypes = domain.EntityDefinitions.Select(ed => ed.EntityType).ToHashSet();

        foreach (var entityDefinition in domain.EntityDefinitions)
        {
            if (entityDefinition.ParentEntityType != null && !entityTypes.Contains(entityDefinition.ParentEntityType))
            {
                throw new ArgumentException($"Parent entity type '{entityDefinition.ParentEntityType}' for entity '{entityDefinition.Name}' does not exist in the provided entity definitions.");
            }
        }

        _logger.LogInformation("Creating domain {DomainName} with {EntityCount} entity type definitions.",
            domain.Name, domain.EntityDefinitions.Length);

        var dom = new Domain
        {
            DomainId = Guid.NewGuid(),
            Name = domain.Name,
            EntityDefinitions = Array.ConvertAll(domain.EntityDefinitions, ed => new EntityDefinition
            {
                EntityDefinitionId = Guid.NewGuid(),
                Name = ed.Name,
                EntityType = ed.EntityType,
                ParentEntityType = ed.ParentEntityType
            })
        };

        await _storage.CreateDomain(tenantId, dom);

        return dom;
    }

    public async Task<Application> CreateApplication(Guid tenantId, Guid domainId, CreateApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

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
            ApplicationId = Guid.NewGuid(),
            Name = application.Name,
            ApplicationType = application.ApplicationType,
            EntityDefinitions = Array.ConvertAll(application.EntityDefinitions, ed => new EntityDefinition
            {
                EntityDefinitionId = Guid.NewGuid(),
                Name = ed.Name,
                EntityType = ed.EntityType,
                ParentEntityType = ed.ParentEntityType
            })
        };

        await _storage.CreateApplication(tenantId, domainId, app);

        return app;
    }

    public Application[] GetApplications()
    {
        return [];
    }
}
