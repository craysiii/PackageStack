namespace PackageStack.Common.Models;

public class PackageConfig
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Version { get; set; }
    [Required]
    public OwnerType OwnerType { get; set; }
    [Range(0, 99)]
    public uint Rank { get; set; }
}