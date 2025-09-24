using System;

namespace Ferrio.EntityMap.Prototype.Api.Services.Models;

public class CreateDomain
{
    public required string Name { get; set; }

    public required CreateEntityDefinition[] EntityDefinitions { get; set; }
}
