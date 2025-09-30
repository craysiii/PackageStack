using System.Management.Automation;
using PackageStack.Common.Models;

namespace PackageStack.Module;

[Cmdlet(VerbsCommon.New, "PackageStackComputerConfiguration")]
[OutputType(typeof(ComputerAccount))]
public class NewPackageStackComputerConfiguration : PSCmdlet
{
    [Parameter(
        ParameterSetName = "Local Computer",
        Mandatory = true,
        Position = 0,
        HelpMessage = "The name to assign the computer - must include random attribute e.g. 'DOMAIN-%RAND:5'"
    )]
    [ValidateNotNullOrEmpty]
    public string? LocalComputerName { get; set; }
    
    [Parameter(
        ParameterSetName = "Domain Join",
        Mandatory = true,
        HelpMessage = "The name to assign the computer - must include random attribute e.g. 'DOMAIN-%RAND:5'"
    )]
    [ValidateNotNullOrEmpty]
    public string? DomainComputerName { get; set; }
    
    [Parameter(
        ParameterSetName = "Domain Join",
        Mandatory = true,
        HelpMessage = "Domain account username used to join to Active Directory"
    )]
    [ValidateNotNullOrEmpty]
    public string? Account { get; set; }
    
    [Parameter(
        ParameterSetName = "Domain Join",
        Mandatory = true,
        HelpMessage = "Domain account password used to join to Active Directory"
    )]
    [ValidateNotNullOrEmpty]
    public string? Password { get; set; }
    
    [Parameter(
        ParameterSetName = "Domain Join",
        Mandatory = true,
        HelpMessage = "Active Directory domain name used to join to Active Directory"
    )]
    [ValidateNotNullOrEmpty]
    public string? DomainName { get; set; }
    
    [Parameter(
        ParameterSetName = "Domain Join",
        Mandatory = false,
        HelpMessage = "Organizational unit to place the computer when joining Active Directory"
    )]
    [ValidateNotNullOrEmpty]
    public string? OrganizationalUnit { get; set; }
    
    protected override void BeginProcessing()
    {
        WriteDebug("Started processing Computer configuration(s)");
    }

    protected override void ProcessRecord()
    {
        WriteDebug($"Processing Computer configuration with computer name {LocalComputerName ?? DomainComputerName}");
        
        WriteObject(
            new ComputerAccount
            {
                ComputerName = (LocalComputerName ?? DomainComputerName)!,
                Account = Account,
                Password = Password,
                DomainName = DomainName,
                AccountOU = OrganizationalUnit
            }
        );
    }

    protected override void EndProcessing()
    {
        WriteDebug("Stopped processing Computer configuration(s)");
    }
}