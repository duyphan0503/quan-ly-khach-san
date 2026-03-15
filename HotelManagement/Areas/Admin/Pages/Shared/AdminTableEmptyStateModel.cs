namespace HotelManagement.Areas.Admin.Pages.Shared;

/// <summary>
/// PageModel xử lý trang quản trị 'AdminTableEmptyStateModel'.
/// </summary>
public sealed class AdminTableEmptyStateModel
{
    public int ColSpan { get; set; }
    public string Icon { get; set; } = "solar:folder-error-bold-duotone";
    public string Title { get; set; } = "Không có dữ liệu";
    public string? Description { get; set; }
    public string? CreatePage { get; set; }
    public string? CreateText { get; set; }
}
