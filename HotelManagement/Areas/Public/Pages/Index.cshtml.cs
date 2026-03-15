using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Public.Pages;

/// <summary>
/// PageModel xử lý trang công khai 'Index.cshtml'.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;

    /// <summary>
    /// Khởi tạo lớp IndexModel và nạp các dependency cần thiết.
    /// </summary>
    public IndexModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public List<RoomType> FeaturedRoomTypes { get; set; } = new();

    /// <summary>
    /// Xử lý yêu cầu GET để nạp dữ liệu và hiển thị trang.
    /// </summary>
    public async Task OnGetAsync()
    {
        // Use Service instead of Repository to follow N-Tier architecture
        FeaturedRoomTypes = await _roomService.GetFeaturedRoomTypesAsync(4);
    }
}
