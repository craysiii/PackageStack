// ReSharper disable InconsistentNaming
namespace PackageStack.Common.Models;

public class OOBE
{
    public bool? EnableCortanaVoice { get; set; }
    [JsonPropertyName("hide_oobe")]
    public bool? HideOOBE { get; set; }
}