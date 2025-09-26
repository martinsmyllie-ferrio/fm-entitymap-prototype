using System;

namespace Ferrio.EntityMap.Prototype.Api.Services.Models;

public class CreateEntityMap
{
    public Guid SourceEnvironmentId { get; set; }

    public required string SourceType { get; set; }

    public required string SourceEntityId { get; set; }

    public Guid TargetEnvironmentId { get; set; }

    public required string TargetType { get; set; }

    public required string TargetEntityId { get; set; }
}
