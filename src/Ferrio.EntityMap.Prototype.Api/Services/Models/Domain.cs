using System;

namespace Ferrio.EntityMap.Prototype.Api.Services.Models;

public class Domain
{
    public Guid DomainId { get; init; }

    public required string Name { get; init; }

    public EntityDefinition[]? EntityDefinitions { get; init; }
}
