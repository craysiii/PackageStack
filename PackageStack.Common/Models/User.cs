namespace PackageStack.Common.Models;

public class User
{
    [Required]
    public required string Username { get; set; }
    [Required]
    public required string Password { get; set; }
    public UserGroup Group { get; set; } = UserGroup.Administrators;
    public string? DisplayName { get; set; }
    public string? HomeDirectory { get; set; }
}