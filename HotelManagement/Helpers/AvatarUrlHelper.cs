namespace HotelManagement.Helpers;

public static class AvatarUrlHelper
{
    public const string DefaultAvatar = "/images/default-avatar.jpg";

    public static string? Normalize(string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
        {
            return null;
        }

        var trimmed = avatarUrl.Trim();
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return trimmed.StartsWith("/") ? trimmed : "/" + trimmed.TrimStart('/');
    }

    public static string ResolveOrDefault(string? avatarUrl)
        => Normalize(avatarUrl) ?? DefaultAvatar;
}
