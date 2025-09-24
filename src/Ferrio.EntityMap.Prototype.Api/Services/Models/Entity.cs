using System;

namespace Ferrio.EntityMap.Prototype.Api.Services.Models;

public class Entity
{
    public required string Id { get; set; }

    public string? ParentId { get; set; }

    public string? Name { get; set; }

    public required string EntityType { get; set; }
}
