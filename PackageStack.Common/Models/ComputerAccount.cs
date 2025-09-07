// ReSharper disable InconsistentNaming
namespace PackageStack.Common.Models;

public class ComputerAccount
{
    [Required]
    public required string ComputerName { get; set; }
    public string? Account { get; set; }
    public string? Password { get; set; }
    public string? DomainName { get; set; }
    [JsonPropertyName("account_ou")]
    public string? AccountOU { get; set; }
}