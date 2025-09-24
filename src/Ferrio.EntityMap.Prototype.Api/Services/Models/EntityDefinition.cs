using System;

namespace Ferrio.EntityMap.Prototype.Api.Services.Models;

public class EntityDefinition
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string EntityType { get; set; }
    public string? ParentEntityType { get; set; }
}
