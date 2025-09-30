using System.Management.Automation;
using PackageStack.Common.Models;

namespace PackageStack.Module;

[Cmdlet(VerbsCommon.New, "PackageStackPackageFileConfiguration")]
[OutputType(typeof(PackageFile))]
public class NewPackageStackPackageFileConfiguration : PSCmdlet
{
    [Parameter(
        ParameterSetName = "Url",
        Mandatory = true,
        Position = 0,
        HelpMessage = "Name of the package file"
    )]
    [Parameter(
        ParameterSetName = "Base64",
        Mandatory = true,
        Position = 0,
        HelpMessage = "Name of the package file"
    )]
    [Parameter(
        ParameterSetName = "Path",
        Mandatory = true,
        Position = 0,
        HelpMessage = "Name of the package file"
    )]
    [ValidateNotNullOrEmpty]
    public required string Name { get; set; }
    
    [Parameter(
        ParameterSetName = "Url",
        Mandatory = true,
        HelpMessage = "Url to download the package file"
    )]
    [ValidateNotNullOrEmpty]
    public string? Url { get; set; }
    
    [Parameter(
        ParameterSetName = "Base64",
        Mandatory = true,
        HelpMessage = "Text from which to decode the package file"
    )]
    [ValidateNotNullOrEmpty]
    public string? Base64 { get; set; }
    
    [Parameter(
        ParameterSetName = "Path",
        Mandatory = true,
        HelpMessage = "Path from which to copy the package file"
    )]
    [ValidateNotNullOrEmpty]
    public string? Path { get; set; }
    
    protected override void BeginProcessing()
    {
        WriteDebug("Started processing PackageFile configuration(s)");
    }
    
    protected override void ProcessRecord()
    {
        WriteDebug($"Processing PackageFile configuration with Name {Name}");
        
        WriteObject(new PackageFile
        {
            Name = Name,
            Url = Url,
            Base64 = Base64,
            Path = Path
        });
    }
    protected override void EndProcessing()
    {
        WriteDebug("Stopped processing PackageFile configuration(s)");
    }
}