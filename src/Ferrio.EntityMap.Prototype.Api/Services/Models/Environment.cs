using System;

namespace Ferrio.EntityMap.Prototype.Api.Services.Models;

public class Environment
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
