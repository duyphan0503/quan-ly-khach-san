using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Public.Pages.Rooms;

/// <summary>
/// PageModel xử lý trang công khai 'Details.cshtml'.
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IRoomService _roomService;

    /// <summary>
    /// Khởi tạo lớp DetailsModel và nạp các dependency cần thiết.
    /// </summary>
    public DetailsModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public Room Room { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public DateTime? CheckIn { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? CheckOut { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Guests { get; set; }

    /// <summary>
    /// Xử lý yêu cầu GET để nạp dữ liệu và hiển thị trang.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room == null)
        {
            return NotFound();
        }

        Room = room;
        return Page();
    }
}
