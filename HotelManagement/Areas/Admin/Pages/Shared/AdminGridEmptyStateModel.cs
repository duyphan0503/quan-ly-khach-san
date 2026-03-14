namespace HotelManagement.Areas.Admin.Pages.Shared;

public sealed class AdminGridEmptyStateModel
{
    public string Icon { get; set; } = "solar:folder-error-bold-duotone";
    public string Title { get; set; } = "Không có dữ liệu";
    public string? Description { get; set; }
    public string? CreatePage { get; set; }
    public string? CreateText { get; set; }
    public string ContainerClass { get; set; } =
        "col-span-full py-20 text-center bg-surface-2/50 rounded-2xl border-2 border-dashed border-bdr-strong";
}
