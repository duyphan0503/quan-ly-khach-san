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
/// Xử lý tạo mới phòng: nạp dữ liệu tham chiếu, nhận ảnh upload và lưu bản ghi phòng vào hệ thống.
/// </summary>
public class CreateModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ phòng và môi trường web để lưu ảnh vào wwwroot.
    /// </summary>
    public CreateModel(IRoomService roomService, IWebHostEnvironment env)
    {
        _roomService = roomService;
        _env = env;
    }

    [BindProperty]
    public Room Room { get; set; } = new Room();

    [BindProperty]
    public IFormFile? UploadImage { get; set; }

    public SelectList RoomTypes { get; set; } = default!;
    public IEnumerable<SelectListItem> RoomStatuses { get; private set; } = [];

    /// <summary>
    /// Nạp danh sách loại phòng và trạng thái để render form tạo mới.
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        var types = await _roomService.GetRoomTypesAsync();
        RoomTypes = new SelectList(types, "Id", "Name");
        RoomStatuses = BuildRoomStatusSelectList(Room.Status);
        return Page();
    }

    /// <summary>
    /// Kiểm tra dữ liệu, lưu ảnh (nếu có) và tạo phòng mới.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        // Room.RoomType là navigation property chỉ phục vụ hiển thị; khi tạo mới chỉ cần RoomTypeId.
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

        var result = await _roomService.CreateAsync(Room);
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
