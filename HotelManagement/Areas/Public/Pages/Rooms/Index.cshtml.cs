using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Public.Pages.Rooms;

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

    public List<Room> AvailableRooms { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? RoomTypeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? CheckIn { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? CheckOut { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Guests { get; set; }
    
    public List<RoomType> RoomTypes { get; set; } = new();

    /// <summary>
    /// Xử lý yêu cầu GET để nạp dữ liệu và hiển thị trang.
    /// </summary>
    public async Task OnGetAsync()
    {
        RoomTypes = await _roomService.GetRoomTypesAsync();
        
        AvailableRooms = await _roomService.GetAvailableRoomsAsync(CheckIn, CheckOut, Guests, RoomTypeFilter);
    }
}
