using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;

namespace HotelManagement.Areas.Admin.Pages.RoomTypes;

public class DetailsModel : PageModel
{
    private readonly IRoomService _roomService;

    public DetailsModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public RoomType RoomType { get; set; } = default!;

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
