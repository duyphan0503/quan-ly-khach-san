using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagement.Areas.Admin.Pages.Rooms;

[Authorize(Roles = "Manager")]
/// <summary>
/// Xử lý cập nhật phòng hiện hữu, bao gồm thay ảnh đại diện và thao tác xóa phòng.
/// </summary>
public class EditModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ phòng và môi trường web để lưu ảnh upload.
    /// </summary>
    public EditModel(IRoomService roomService, IWebHostEnvironment env)
    {
        _roomService = roomService;
        _env = env;
    }

    [BindProperty]
    public Room Room { get; set; } = default!;

    [BindProperty]
    public IFormFile? UploadImage { get; set; }

    public SelectList RoomTypes { get; set; } = default!;
    public IEnumerable<SelectListItem> RoomStatuses { get; private set; } = [];

    /// <summary>
    /// Nạp phòng theo id cùng dữ liệu tham chiếu để hiển thị form chỉnh sửa.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var room = await _roomService.GetByIdAsync(id.Value);
        if (room == null) return NotFound();

        Room = room;
        var types = await _roomService.GetRoomTypesAsync();
        RoomTypes = new SelectList(types, "Id", "Name");
        RoomStatuses = BuildRoomStatusSelectList(Room.Status);
        
        return Page();
    }

    /// <summary>
    /// Kiểm tra dữ liệu, xử lý ảnh mới (nếu có) và cập nhật thông tin phòng.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Room.RoomType");
        
        if (!ModelState.IsValid)
        {
            var types = await _roomService.GetRoomTypesAsync();
            RoomTypes = new SelectList(types, "Id", "Name");
            RoomStatuses = BuildRoomStatusSelectList(Room.Status);
            return Page();
        }

        if (UploadImage != null)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(UploadImage.FileName);
            var filePath = Path.Combine(_env.WebRootPath, "uploads", "rooms", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await UploadImage.CopyToAsync(stream);
            }
            Room.ImageUrl = $"/uploads/rooms/{fileName}";
        }

        var result = await _roomService.UpdateAsync(Room);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Message);
        var typesRe = await _roomService.GetRoomTypesAsync();
        RoomTypes = new SelectList(typesRe, "Id", "Name");
        RoomStatuses = BuildRoomStatusSelectList(Room.Status);
        return Page();
    }

    /// <summary>
    /// Xóa phòng theo id hiện tại và điều hướng về danh sách hoặc quay lại màn hình sửa khi thất bại.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (Room.Id == 0) return NotFound();

        var result = await _roomService.DeleteAsync(Room.Id);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("./Index");
        }

        TempData["ErrorMessage"] = result.Message;
        return RedirectToPage("./Edit", new { id = Room.Id });
    }

    private static IEnumerable<SelectListItem> BuildRoomStatusSelectList(RoomStatus selectedStatus)
    {
        return new List<SelectListItem>
        {
            new() { Value = ((int)RoomStatus.Available).ToString(), Text = "Trống" },
            new() { Value = ((int)RoomStatus.Occupied).ToString(), Text = "Đang ở" },
            new() { Value = ((int)RoomStatus.Maintenance).ToString(), Text = "Bảo trì" },
            new() { Value = ((int)RoomStatus.Reserved).ToString(), Text = "Đã đặt" }
        }.Select(item =>
        {
            item.Selected = item.Value == ((int)selectedStatus).ToString();
            return item;
        });
    }
}
