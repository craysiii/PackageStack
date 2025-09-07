// ReSharper disable InconsistentNaming
namespace PackageStack.Common.Models;

public class Azure
{
    public string Authority { get; set; } = "https://login.microsoftonline.com/common";
    [Required]
    [JsonPropertyName("bprt")]
    public required string BPRT { get; set; }
}