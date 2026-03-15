using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;

namespace HotelManagement.Areas.Admin.Pages.RoomTypes;

/// <summary>
/// Xử lý tạo mới loại phòng và lưu ảnh đại diện (nếu được tải lên).
/// </summary>
public class CreateModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ phòng và môi trường web để thao tác thư mục upload.
    /// </summary>
    public CreateModel(IRoomService roomService, IWebHostEnvironment env)
    {
        _roomService = roomService;
        _env = env;
    }

    [BindProperty]
    public RoomType RoomType { get; set; } = new();

    [BindProperty]
    public IFormFile? UploadImage { get; set; }

    /// <summary>
    /// Hiển thị form tạo loại phòng.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Kiểm tra dữ liệu nhập, lưu file ảnh và tạo loại phòng mới.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (UploadImage != null)
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "roomtypes");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(UploadImage.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await UploadImage.CopyToAsync(stream);
            }
            RoomType.ImageUrl = $"/uploads/roomtypes/{fileName}";
        }

        var (success, message) = await _roomService.CreateRoomTypeAsync(RoomType);
        
        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToPage("./Index");
        }

        TempData["ErrorMessage"] = message;
        return Page();
    }
}
