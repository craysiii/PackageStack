using System;
using System.Management.Automation;
// ReSharper disable InconsistentNaming

namespace PackageStack.Module;

[Cmdlet(VerbsCommon.New, "PackageStackAzureConfiguration")]
[OutputType(typeof(Common.Models.Azure))]
public class NewPackageStackAzureConfiguration : PSCmdlet
{
    [Parameter(
        Mandatory = false,
        HelpMessage = "The authority server"
    )]
    public string Authority { get; set; } = "https://login.microsoftonline.com/common";
    
    [Parameter(
        Mandatory = true,
        Position = 0,
        HelpMessage = "Bulk Primary Refresh Token - can be generated using WICD or AAD-Internals module"
        )]
    [ValidateNotNullOrEmpty]
    public required string BPRT { get; set; }

    protected override void BeginProcessing()
    {
        WriteDebug("Started processing Azure configuration(s)");
    }

    protected override void ProcessRecord()
    {
        WriteDebug($"Processing Azure configuration with Authority {Authority} BPRT {BPRT[..Math.Min(BPRT.Length, 20)]}");
        
        WriteObject(
            new Common.Models.Azure
            {
                Authority = Authority,
                BPRT = BPRT
            }
        );
    }

    protected override void EndProcessing()
    {
        WriteDebug("Stopped processing Azure configuration(s)");
    }
}