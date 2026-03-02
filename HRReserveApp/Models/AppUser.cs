namespace HRReserveApp.Models;

public sealed class AppUser
{
    public string Login { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
