namespace HotelManagement.Areas.Admin.Pages.Shared;

public sealed class AdminPaginationModel
{
    public string AspPage { get; set; } = "./Index";
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int CurrentCount { get; set; }
    public int TotalCount { get; set; }
    public string ItemLabel { get; set; } = "mục";
    public string ContainerClass { get; set; } =
        "px-6 py-4 bg-surface-2/60 backdrop-blur-2xl border-t border-white/5 flex flex-col md:flex-row justify-between items-center gap-4";
    public Dictionary<string, string?> RouteValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
