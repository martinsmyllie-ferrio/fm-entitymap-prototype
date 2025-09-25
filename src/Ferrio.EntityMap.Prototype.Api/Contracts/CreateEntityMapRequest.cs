using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public class CreateEntityMapRequest
{
    public required EntityReference Source { get; set; }

    public required EntityReference Target { get; set; }
}

public class EntityReference
{
    public Guid EnvironmentId { get; set; }

    public required string EntityType { get; set; }

    public required string Id { get; set; }
}