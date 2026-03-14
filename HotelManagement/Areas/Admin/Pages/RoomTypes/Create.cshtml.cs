using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;

namespace HotelManagement.Areas.Admin.Pages.RoomTypes;

public class CreateModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IWebHostEnvironment _env;

    public CreateModel(IRoomService roomService, IWebHostEnvironment env)
    {
        _roomService = roomService;
        _env = env;
    }

    [BindProperty]
    public RoomType RoomType { get; set; } = new();

    [BindProperty]
    public IFormFile? UploadImage { get; set; }

    public void OnGet()
    {
    }

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
