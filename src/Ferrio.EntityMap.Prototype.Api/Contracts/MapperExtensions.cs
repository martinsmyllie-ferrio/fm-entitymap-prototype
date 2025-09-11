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
}
