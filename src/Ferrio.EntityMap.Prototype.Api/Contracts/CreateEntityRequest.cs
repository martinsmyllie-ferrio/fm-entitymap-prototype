using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public class CreateEntityRequest
{
    public required string Id { get; set; }

    public string? ParentId { get; set; }

    public string? ParentEntityType { get; set; }

    public string? Name { get; set; }

    public required string EntityType { get; set; }
}
