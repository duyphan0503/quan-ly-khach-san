using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;

namespace HotelManagement.Areas.Admin.Pages.RoomTypes;

/// <summary>
/// Xử lý cập nhật loại phòng, bao gồm thay ảnh đại diện và thao tác xóa bản ghi.
/// </summary>
public class EditModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ phòng và môi trường web để quản lý file ảnh.
    /// </summary>
    public EditModel(IRoomService roomService, IWebHostEnvironment env)
    {
        _roomService = roomService;
        _env = env;
    }

    [BindProperty]
    public RoomType RoomType { get; set; } = default!;

    [BindProperty]
    public IFormFile? UploadImage { get; set; }

    /// <summary>
    /// Nạp dữ liệu loại phòng theo id để hiển thị form chỉnh sửa.
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

    /// <summary>
    /// Kiểm tra dữ liệu, cập nhật thông tin loại phòng và xử lý thay ảnh nếu có upload mới.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var existingRoomType = await _roomService.GetRoomTypeByIdAsync(RoomType.Id);
        if (existingRoomType == null)
            return NotFound();

        // Cập nhật dữ liệu text/number trước khi xử lý ảnh.
        existingRoomType.Name = RoomType.Name;
        existingRoomType.BasePrice = RoomType.BasePrice;
        existingRoomType.MaxOccupancy = RoomType.MaxOccupancy;
        existingRoomType.Amenities = RoomType.Amenities;
        existingRoomType.Description = RoomType.Description;

        // Nếu có ảnh mới: xóa ảnh cũ (nếu tồn tại) rồi lưu ảnh mới.
        if (UploadImage != null)
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "roomtypes");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            // Dọn file ảnh cũ để tránh phát sinh file mồ côi trên ổ đĩa.
            if (!string.IsNullOrEmpty(existingRoomType.ImageUrl))
            {
                var oldFilePath = Path.Combine(_env.WebRootPath, existingRoomType.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(UploadImage.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await UploadImage.CopyToAsync(stream);
            }

            existingRoomType.ImageUrl = $"/uploads/roomtypes/{fileName}";
        }

        var (success, message) = await _roomService.UpdateRoomTypeAsync(existingRoomType);
        
        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToPage("./Index");
        }

        TempData["ErrorMessage"] = message;
        return Page();
    }

    /// <summary>
    /// Xóa loại phòng theo id và phản hồi trạng thái bằng TempData.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var (success, message) = await _roomService.DeleteRoomTypeAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }
        return RedirectToPage("./Index");
    }
}

