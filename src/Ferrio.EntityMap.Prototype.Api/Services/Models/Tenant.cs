using System;

namespace Ferrio.EntityMap.Prototype.Api.Services.Models;

public class Tenant
{
    public Guid TenantId { get; init; }

    public required string Name { get; init; }
}
