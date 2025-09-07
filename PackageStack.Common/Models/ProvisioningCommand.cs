namespace PackageStack.Common.Models;

public class ProvisioningCommand
{
    [Required]
    public required string CommandLine { get; set; }
    public PackageFile? CommandFile { get; set; }
    public List<PackageFile>? Dependencies { get; set; }
    public bool? ContinueInstall { get; set; }
    public bool? RestartRequired { get; set; }
    public int? ReturnCodeRestart { get; set; }
    public int? ReturnCodeSuccess { get; set; }
}