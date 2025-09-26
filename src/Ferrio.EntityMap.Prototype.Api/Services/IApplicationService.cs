using System;
using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public interface IApplicationService
{
    Task<Tenant> CreateTenant(Tenant tenant);

    Task<Domain> CreateDomain(Guid tenantId, CreateDomain domain);

    Task<Application> CreateApplication(Guid tenantId, Guid domainId, CreateApplication application);
}
