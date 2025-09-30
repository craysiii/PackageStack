using System;
using System.Management.Automation;
using PackageStack.Common.Enums;
using PackageStack.Common.Models;

namespace PackageStack.Module;

[Cmdlet(VerbsCommon.New, "PackageStackPackageConfiguration")]
[OutputType(typeof(PackageConfig))]
public class NewPackageStackPackageConfiguration : PSCmdlet
{
    [Parameter(
        Mandatory = false,
        HelpMessage = "Unique identifier for the package"
    )]
    public Guid? Id { get; set; }
    
    [Parameter(
        Mandatory = true,
        HelpMessage = "Display name for the package"
    )]
    [ValidateNotNullOrEmpty]
    public required string Name { get; set; }
    
    [Parameter(
        Mandatory = true,
        HelpMessage = "Version string for the package - must be in the format major.minor e.g. 1.2"
    )]
    [ValidatePattern(@"^\d+\.\d{0,2}$")]
    public required string Version { get; set; }
    
    [Parameter(
        Mandatory = true,
        HelpMessage = "Package builder type - e.g. ITAdmin"
    )]
    public OwnerType OwnerType { get; set; }
    
    [Parameter(
        Mandatory = true,
        HelpMessage = "Priority for the package to be installed - lower value is higher priority"
    )]
    [ValidateRange(0, 99)]
    public uint Rank { get; set; }

    protected override void BeginProcessing()
    {
        WriteDebug("Started processing Package configuration(s)");
    }

    protected override void ProcessRecord()
    {
        WriteDebug($"Processing Package configuration {Name}");
        
        WriteObject(
            new PackageConfig
            {
                Id = Id ?? Guid.NewGuid(),
                Name = Name,
                Version = Version,
                OwnerType = OwnerType,
                Rank = Rank
            }
        );
    }

    protected override void EndProcessing()
    {
        WriteDebug("Started processing Package configuration(s)");
    }
}