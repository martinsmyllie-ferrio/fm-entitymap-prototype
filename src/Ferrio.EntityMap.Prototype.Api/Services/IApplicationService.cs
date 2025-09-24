using System;
using Ferrio.EntityMap.Prototype.Api.Services.Models;

namespace Ferrio.EntityMap.Prototype.Api.Services;

public interface IApplicationService
{
    Task<Application> CreateApplication(CreateApplication application);
}
