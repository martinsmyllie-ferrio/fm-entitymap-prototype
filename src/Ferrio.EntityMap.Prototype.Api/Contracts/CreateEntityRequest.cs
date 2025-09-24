using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public class CreateEntityRequest
{
    public required string ReferenceId { get; set; }

    public string? ParentReferenceId { get; set; }

    public string? Name { get; set; }

    public required string EntityType { get; set; }
}
