using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public class CreateDomainRequest
{
    public required string Name { get; set; }

    public CreateEntityDefinitionRequest[]? EntityDefinitions { get; set; }
}
