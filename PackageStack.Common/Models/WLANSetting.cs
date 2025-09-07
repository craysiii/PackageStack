// ReSharper disable InconsistentNaming
namespace PackageStack.Common.Models;

public class WLANSetting
{
    [JsonPropertyName("ssid")]
    public required string SSID { get; set; }
    public bool AutoConnect { get; set; } = true;
    public bool HiddenNetwork { get; set; }
    [Required]
    public SecurityType SecurityType { get; set; }
    public string? SecurityKey { get; set; }
}