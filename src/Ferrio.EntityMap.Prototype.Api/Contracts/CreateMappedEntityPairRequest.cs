using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public class CreateMappedEntityPairRequest
{
    public Guid SourceEnvironmentId { get; set; }

    public Guid TargetEnvironmentId { get; set; }
    
    public required CreateEntityRequest Source { get; set; }

    public required CreateEntityRequest Target { get; set; }
}
