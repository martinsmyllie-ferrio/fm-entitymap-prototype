using System;

namespace Ferrio.EntityMap.Prototype.Api.Services.Models;

public class CreateMappedEntities
{
    public Guid SourceEnvironmentId { get; set; }

    public Guid TargetEnvironmentId { get; set; }
    
    public required Entity SourceEntity { get; set; }

    public required Entity TargetEntity { get; set; }
}


public class MappedEntities
{
    public Guid SourceEnvironmentId { get; set; }

    public Guid TargetEnvironmentId { get; set; }

    public required Entity SourceEntity { get; set; }

    public required Entity TargetEntity { get; set; }
}