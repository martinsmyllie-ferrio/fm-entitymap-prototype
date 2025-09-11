using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public class CreateApplicationRequest
{
    public required string Name { get; set; }

    public required string ApplicationType { get; set; }

    public CreateEntityDefinitionRequest[]? EntityDefinitions { get; set; }
}
