namespace HotelManagement.Helpers;

/// <summary>
/// Chuẩn hóa và resolve đường dẫn avatar để UI luôn hiển thị được ảnh.
/// </summary>
public static class AvatarUrlHelper
{
    // Ảnh fallback khi user/guest chưa có avatar.
    public const string DefaultAvatar = "/images/default-avatar.jpg";

    // Chuẩn hóa URL: trim, giữ nguyên absolute URL, convert relative thành path bắt đầu bằng '/'.
    /// <summary>
    /// Chuẩn hóa giá trị đầu vào về định dạng hợp lệ trước khi sử dụng.
    /// </summary>
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

    // Trả về URL hợp lệ hoặc ảnh mặc định.
    /// <summary>
    /// Trả về giá trị hợp lệ hoặc dùng giá trị mặc định khi dữ liệu trống.
    /// </summary>
    public static string ResolveOrDefault(string? avatarUrl)
        => Normalize(avatarUrl) ?? DefaultAvatar;
}
