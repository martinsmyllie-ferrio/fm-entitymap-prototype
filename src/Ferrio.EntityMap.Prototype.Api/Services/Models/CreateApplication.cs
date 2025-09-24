using System;

namespace Ferrio.EntityMap.Prototype.Api.Services.Models;

public class CreateApplication
{
    public required string Name { get; set; }

    public required string ApplicationType { get; set; }

    public required CreateEntityDefinition[] EntityDefinitions { get; set; }
}
