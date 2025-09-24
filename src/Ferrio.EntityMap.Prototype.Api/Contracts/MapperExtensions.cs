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
            EntityDefinitions = createApplicationRequest.EntityDefinitions?.ToModel() ?? Array.Empty<Services.Models.CreateEntityDefinition>()
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

    public static Services.Models.CreateEntity ToModel(this CreateEntityRequest createEntityRequest)
    {
        return new Services.Models.CreateEntity
        {
            ReferenceId = createEntityRequest.ReferenceId,
            ParentReferenceId = createEntityRequest.ParentReferenceId,
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
            SourceReferenceId = createEntityMapRequest.Source.ReferenceId,
            TargetEnvironmentId = createEntityMapRequest.Target.EnvironmentId,
            TargetType = createEntityMapRequest.Target.EntityType,
            TargetReferenceId = createEntityMapRequest.Target.ReferenceId
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
                ReferenceId = createMappedEntityPairRequest.Source.ReferenceId,
                ParentReferenceId = createMappedEntityPairRequest.Source.ParentReferenceId,
                EntityType = createMappedEntityPairRequest.Source.EntityType
            },
            TargetEnvironmentId = createMappedEntityPairRequest.TargetEnvironmentId,
            TargetEntity = new()
            {
                Name = createMappedEntityPairRequest.Target.Name,
                ReferenceId = createMappedEntityPairRequest.Target.ReferenceId,
                ParentReferenceId = createMappedEntityPairRequest.Target.ParentReferenceId,
                EntityType = createMappedEntityPairRequest.Target.EntityType
            },
        };
    }
}
