using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public class CreateEntityDefinitionRequest
{
    public required string Name { get; set; }

    public required string EntityType { get; set; }

    public string? ParentEntityType { get; set; }
}
