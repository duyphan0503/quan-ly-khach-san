namespace HotelManagement.Areas.Admin.Pages.Shared;

/// <summary>
/// PageModel xử lý trang quản trị 'AdminPageHeaderModel'.
/// </summary>
public sealed class AdminPageHeaderModel
{
    public string Icon { get; set; } = "solar:widget-bold-duotone";
    public string Title { get; set; } = "Tiêu đề trang";
    public string Subtitle { get; set; } = string.Empty;
    public string HeadingTag { get; set; } = "h1";
    public string WrapperClass { get; set; } = "space-y-1";
}
