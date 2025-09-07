namespace PackageStack.Common.Models;

public class PackageFile
{
    [Required]
    public required string Name { get; set; }
    public string? Url { get; set; }
    public string? Base64 { get; set; }
}