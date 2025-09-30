using System.Management.Automation;
using PackageStack.Common.Enums;
using PackageStack.Common.Models;
// ReSharper disable InconsistentNaming

namespace PackageStack.Module;

[Cmdlet(VerbsCommon.New, "PackageStackWLANSettingConfiguration")]
[OutputType(typeof(WLANSetting))]
public class NewPackageStackWLANSettingConfiguration : PSCmdlet
{
    [Parameter(
        Mandatory = true,
        Position = 0,
        HelpMessage = "SSID for the WLAN"
    )]
    [ValidateNotNullOrEmpty]
    public required string SSID { get; set; }
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "Should the WLAN be automatically connected to"
    )]
    public bool AutoConnect { get; set; } = true;
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "Does the WLAN broadcast its SSID"
    )]
    public bool HiddenNetwork { get; set; }
    
    [Parameter(
        Mandatory = true,
        HelpMessage = "Security connection type for the WLAN"
    )]
    public required SecurityType SecurityType { get; set; }
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "Password for the WLAN if required by SecurityType"
    )]
    [ValidateNotNullOrEmpty]
    public string? SecurityKey { get; set; }
    
    protected override void BeginProcessing()
    {
        WriteDebug("Started processing WLAN configuration(s)");
    }

    protected override void ProcessRecord()
    {
        WriteDebug($"Processing WLAN configuration for WLAN {SSID}");
        
        WriteObject(
            new WLANSetting
            {
                SSID = SSID,
                AutoConnect = AutoConnect,
                HiddenNetwork = HiddenNetwork,
                SecurityType = SecurityType,
                SecurityKey = SecurityKey
            }
        );
    }
    
    protected override void EndProcessing()
    {
        WriteDebug("Stopped processing WLAN configuration(s)");
    }
}