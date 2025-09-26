using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public class CreateTenantRequest
{
    public Guid TenantId { get; set; }

    public required string Name { get; set; }
}
