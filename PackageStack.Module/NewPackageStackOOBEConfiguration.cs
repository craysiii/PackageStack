using System.Management.Automation;
using PackageStack.Common.Models;

// ReSharper disable InconsistentNaming

namespace PackageStack.Module;

[Cmdlet(VerbsCommon.New, "PackageStackOOBEConfiguration")]
[OutputType(typeof(OOBE))]
public class NewPackageStackOOBEConfiguration : PSCmdlet
{
    [Parameter(
        Mandatory = false,
        HelpMessage = "Enable Cortana Voice during the out of box experience"
    )]
    public bool? EnableCortanaVoice { get; set; }
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "Skip the out of box experience when setting up a new computer"
    )]
    public bool? HideOOBE { get; set; }

    protected override void BeginProcessing()
    {
        WriteDebug("Started processing OOBE configuration(s)");
    }

    protected override void ProcessRecord()
    {
        WriteDebug($"Processing OOBE configuration with EnableCortanaVoice {EnableCortanaVoice} HideOOBE {HideOOBE}");
        
        WriteObject(
            new OOBE
            {
                EnableCortanaVoice = EnableCortanaVoice,
                HideOOBE = HideOOBE
            }
        );
    }

    protected override void EndProcessing()
    {
        WriteDebug("Stopped processing OOBE configuration(s)");
    }
}