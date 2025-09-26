using System;

namespace Ferrio.EntityMap.Prototype.Api.Services.Models;

public class Application
{
    public Guid ApplicationId { get; set; }
    public required string Name { get; set; }
    public required string ApplicationType { get; set; }
    public EntityDefinition[]? EntityDefinitions { get; set; }
}
