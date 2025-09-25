using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public static class MapperExtensions
{
    public static Services.Models.CreateApplication ToModel(this CreateApplicationRequest createApplicationRequest)
    {
        return new Services.Models.CreateApplication
        {
            Name = createApplicationRequest.Name,
            ApplicationType = createApplicationRequest.ApplicationType,
            EntityDefinitions = createApplicationRequest.EntityDefinitions?.ToModel() ?? []
        };
    }

    public static Services.Models.CreateEntityDefinition[] ToModel(this CreateEntityDefinitionRequest[] createEntityDefinitionRequests)
    {
        var models = new Services.Models.CreateEntityDefinition[createEntityDefinitionRequests.Length];

        for (int i = 0; i < createEntityDefinitionRequests.Length; i++)
        {
            models[i] = new Services.Models.CreateEntityDefinition
            {
                Name = createEntityDefinitionRequests[i].Name,
                EntityType = createEntityDefinitionRequests[i].EntityType,
                ParentEntityType = createEntityDefinitionRequests[i].ParentEntityType
            };
        }
        return models;
    }

    public static Services.Models.CreateEnvironment ToModel(this CreateEnvironmentRequest createEnvironmentRequest)
    {
        return new Services.Models.CreateEnvironment
        {
            Name = createEnvironmentRequest.Name
        };
    }

    public static Services.Models.Entity ToModel(this CreateEntityRequest createEntityRequest)
    {
        return new Services.Models.Entity
        {
            Id = createEntityRequest.Id,
            Parent = createEntityRequest.ParentId != null && createEntityRequest.ParentEntityType != null ? new Services.Models.Entity
            {
                Id = createEntityRequest.ParentId,
                EntityType = createEntityRequest.ParentEntityType
            } : null,
            Name = createEntityRequest.Name,
            EntityType = createEntityRequest.EntityType
        };
    }

    public static Services.Models.CreateEntityMap ToModel(this CreateEntityMapRequest createEntityMapRequest)
    {
        return new Services.Models.CreateEntityMap
        {
            SourceEnvironmentId = createEntityMapRequest.Source.EnvironmentId,
            SourceType = createEntityMapRequest.Source.EntityType,
            SourceEntityId = createEntityMapRequest.Source.Id,
            TargetEnvironmentId = createEntityMapRequest.Target.EnvironmentId,
            TargetType = createEntityMapRequest.Target.EntityType,
            TargetEntityId = createEntityMapRequest.Target.Id
        };
    }

    public static Services.Models.CreateMappedEntities ToModel(this CreateMappedEntityPairRequest createMappedEntityPairRequest)
    {
        return new Services.Models.CreateMappedEntities
        {
            SourceEnvironmentId = createMappedEntityPairRequest.SourceEnvironmentId,
            SourceEntity = new()
            {
                Name = createMappedEntityPairRequest.Source.Name,
                Id = createMappedEntityPairRequest.Source.Id,
                Parent = createMappedEntityPairRequest.Source.ParentId != null && createMappedEntityPairRequest.Source.ParentEntityType != null
                ? new Services.Models.Entity
                {
                    Id = createMappedEntityPairRequest.Source.ParentId,
                    EntityType = createMappedEntityPairRequest.Source.ParentEntityType
                }
                : null,
                EntityType = createMappedEntityPairRequest.Source.EntityType
            },
            TargetEnvironmentId = createMappedEntityPairRequest.TargetEnvironmentId,
            TargetEntity = new()
            {
                Name = createMappedEntityPairRequest.Target.Name,
                Id = createMappedEntityPairRequest.Target.Id,
                Parent = createMappedEntityPairRequest.Target.ParentId != null && createMappedEntityPairRequest.Target.ParentEntityType != null
                ? new Services.Models.Entity
                {
                    Id = createMappedEntityPairRequest.Target.ParentId,
                    EntityType = createMappedEntityPairRequest.Target.ParentEntityType
                }
                : null,
                EntityType = createMappedEntityPairRequest.Target.EntityType
            },
        };
    }

    public static Services.Models.Tenant ToModel(this CreateTenantRequest createTenantRequest)
    {
        return new Services.Models.Tenant
        {
            TenantId = createTenantRequest.TenantId,
            Name = createTenantRequest.Name
        };
    }

    public static Services.Models.CreateDomain ToModel(this CreateDomainRequest createDomainRequest)
    {
        return new Services.Models.CreateDomain
        {
            Name = createDomainRequest.Name,
            EntityDefinitions = createDomainRequest.EntityDefinitions?.ToModel() ?? []
        };
    }

    public static Dictionary<string, string> ToModel(this CreateSettingsRequest[] requests)
    {
        return requests.ToDictionary(r => r.SettingName, r => r.SettingValue);
    }
}
