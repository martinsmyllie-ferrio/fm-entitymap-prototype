using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public class EnvironmentPublishingCapabilities
{
    public Guid TargetEnvironmentId { get; set; }

    /// <summary>
    /// dictionary of entity types, indicating whether by default they publish data to the specified target environment
    /// </summary>
    public required Dictionary<string, bool> PublishCapabilities { get; set; }
}
