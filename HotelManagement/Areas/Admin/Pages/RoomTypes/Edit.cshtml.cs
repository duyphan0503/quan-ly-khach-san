using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;

namespace HotelManagement.Areas.Admin.Pages.RoomTypes;

public class EditModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IWebHostEnvironment _env;

    public EditModel(IRoomService roomService, IWebHostEnvironment env)
    {
        _roomService = roomService;
        _env = env;
    }

    [BindProperty]
    public RoomType RoomType { get; set; } = default!;

    [BindProperty]
    public IFormFile? UploadImage { get; set; }

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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var existingRoomType = await _roomService.GetRoomTypeByIdAsync(RoomType.Id);
        if (existingRoomType == null)
            return NotFound();

        // Update properties
        existingRoomType.Name = RoomType.Name;
        existingRoomType.BasePrice = RoomType.BasePrice;
        existingRoomType.MaxOccupancy = RoomType.MaxOccupancy;
        existingRoomType.Amenities = RoomType.Amenities;
        existingRoomType.Description = RoomType.Description;

        // Xử lý upload ảnh mới
        if (UploadImage != null)
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "roomtypes");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            // Xóa ảnh cũ nếu có
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
