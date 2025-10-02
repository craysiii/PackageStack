using System.Linq;
using System.Management.Automation;
using PackageStack.Common.Models;

namespace PackageStack.Module;

[Cmdlet(VerbsCommon.New, "PackageStackProvisioningCommandConfiguration")]
[OutputType(typeof(ProvisioningCommand))]
public class NewPackageStackProvisioningCommandConfiguration : PSCmdlet
{
    [Parameter(
        Mandatory = true,
        Position = 0,
        HelpMessage = "Provisioning Command command line to execute"
    )]
    [ValidateNotNullOrEmpty]
    public required string CommandLine { get; set; }
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "Provisioning Command primary file to execute"
    )]
    public PackageFile? CommandFile { get; set; }
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "Provisioning Command dependency files to include"
    )]
    public PackageFile[]? Dependencies { get; set; }
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "Continue installation if the Provisioning Command fails"
    )]
    public bool? ContinueInstall { get; set; }
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "Restart required when the Provisioning Command completes"
    )]
    public bool? RestartRequired { get; set; }
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "Return code to expect if Provisioning Command should restart"
    )]
    public int? ReturnCodeRestart { get; set; }
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "Return code to expect if Provisioning Command completes successfully"
    )]   
    public int? ReturnCodeSuccess { get; set; }

    protected override void BeginProcessing()
    {
        WriteDebug("Started processing ProvisioningCommand configuration(s)");
    }

    protected override void ProcessRecord()
    {
        WriteObject(
            new ProvisioningCommand
            {
                CommandLine = CommandLine,
                CommandFile = CommandFile,
                Dependencies = Dependencies?.ToList(),
                ContinueInstall = ContinueInstall,
                RestartRequired = RestartRequired,
                ReturnCodeRestart = ReturnCodeRestart,
                ReturnCodeSuccess = ReturnCodeSuccess
            }
        );
    }
    
    protected override void EndProcessing()
    {
        WriteDebug("Stopped processing ProvisioningCommand configuration(s)");
    }
}