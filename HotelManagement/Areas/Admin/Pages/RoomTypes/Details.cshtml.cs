using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;

namespace HotelManagement.Areas.Admin.Pages.RoomTypes;

/// <summary>
/// Hiển thị thông tin chi tiết của một loại phòng.
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IRoomService _roomService;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ phòng.
    /// </summary>
    public DetailsModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public RoomType RoomType { get; set; } = default!;

    /// <summary>
    /// Nạp loại phòng theo id; nếu không tồn tại thì điều hướng về danh sách kèm thông báo.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var roomType = await _roomService.GetRoomTypeByIdAsync(id);
        if (roomType == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy loại phòng.";
            return RedirectToPage("./Index");
        }

        RoomType = roomType;
        return Page();
    }
}
