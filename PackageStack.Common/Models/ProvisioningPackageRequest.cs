// ReSharper disable InconsistentNaming
namespace PackageStack.Common.Models;

public class ProvisioningPackageRequest
{
    [JsonIgnore]
    public  Guid RequestId { get; init; } = Guid.NewGuid();
    [Required]
    public required ReturnType ReturnType { get; set; }
    public string? ContainerName { get; set; }
    public string? FileName { get; set; }
    [Required]
    public required PackageConfig PackageConfig { get; set; }
    public Azure? Azure { get; set; }
    public ComputerAccount? ComputerAccount { get; set; }
    [JsonPropertyName("oobe")]
    public OOBE? OOBE { get; set; }
    public List<User>? LocalUsers { get; set; }
    public List<ProvisioningCommand>? ProvisioningCommands { get; set; }
    [JsonPropertyName("wlan_settings")]
    public List<WLANSetting>? WLANSettings { get; set; }
    
}