using System.Management.Automation;
using PackageStack.Common.Enums;
using PackageStack.Common.Models;

namespace PackageStack.Module;

[Cmdlet(VerbsCommon.New, "PackageStackUserConfiguration")]
[OutputType(typeof(User))]
public class NewPackageStackUserConfiguration : PSCmdlet
{
    [Parameter(
        Mandatory = true,
        Position = 0,
        HelpMessage = "Username for the user"
    )]
    [ValidateNotNullOrEmpty]
    public required string Username { get; set; }
    
    [Parameter(
        Mandatory = true,
        Position = 1,
        HelpMessage = "Password for the user"    
    )]
    [ValidateNotNullOrEmpty]
    public required string Password { get; set; }
    
    [Parameter(
        Mandatory = false,
        HelpMessage = "User group to assign the user to - defaults to Standard Users"
    )]
    public UserGroup Group { get; set; } = UserGroup.StandardUsers;

    protected override void BeginProcessing()
    {
        WriteDebug("Started processing User configuration(s)");
    }
    
    protected override void ProcessRecord()
    {
        WriteDebug($"Started processing User configuration {Username}");
        
        WriteObject(
            new User
            {
                Username = Username,
                Password = Password,
                Group = Group
            }
        );
    }
    
    protected override void EndProcessing()
    {
        WriteDebug("Stopped processing User configuration(s)");
    }
}