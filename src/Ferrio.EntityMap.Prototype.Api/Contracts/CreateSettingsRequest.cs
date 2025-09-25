using System;

namespace Ferrio.EntityMap.Prototype.Api.Contracts;

public class CreateSettingsRequest
{
    public required string SettingName { get; set; }
    public required string SettingValue { get; set; }
}
