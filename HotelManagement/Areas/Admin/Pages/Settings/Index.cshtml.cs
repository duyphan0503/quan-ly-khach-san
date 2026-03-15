using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Admin.Pages.Settings;

[Authorize(Roles = "Manager")]
/// <summary>
/// PageModel xử lý trang quản trị 'Index.cshtml'.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IWebsiteSettingsService _websiteSettingsService;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Khởi tạo lớp IndexModel và nạp các dependency cần thiết.
    /// </summary>
    public IndexModel(IWebsiteSettingsService websiteSettingsService, IWebHostEnvironment environment)
    {
        _websiteSettingsService = websiteSettingsService;
        _environment = environment;
    }

    [BindProperty]
    public WebsiteSettings Input { get; set; } = new();

    [BindProperty]
    public IFormFile? LogoFile { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Xử lý yêu cầu GET để nạp dữ liệu và hiển thị trang.
    /// </summary>
    public async Task OnGetAsync()
    {
        Input = await _websiteSettingsService.GetCurrentAsync();
    }

    /// <summary>
    /// Xử lý yêu cầu POST, kiểm tra dữ liệu đầu vào và lưu thay đổi.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            return Page();
        }

        if (LogoFile is not null && LogoFile.Length > 0)
        {
            var extension = Path.GetExtension(LogoFile.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".svg", ".png", ".jpg", ".jpeg", ".webp" };
            if (!allowedExtensions.Contains(extension))
            {
                ErrorMessage = "Logo chỉ hỗ trợ định dạng SVG, PNG, JPG, JPEG hoặc WEBP.";
                return Page();
            }

            const long maxLogoSize = 2 * 1024 * 1024;
            if (LogoFile.Length > maxLogoSize)
            {
                ErrorMessage = "Kích thước logo tối đa là 2MB.";
                return Page();
            }

            var brandingDirectory = Path.Combine(_environment.WebRootPath, "images", "branding");
            Directory.CreateDirectory(brandingDirectory);

            var fileName = $"logo-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            var absoluteFilePath = Path.Combine(brandingDirectory, fileName);

            await using (var stream = System.IO.File.Create(absoluteFilePath))
            {
                await LogoFile.CopyToAsync(stream);
            }

            Input.LogoUrl = $"/images/branding/{fileName}";
        }

        var (success, message) = await _websiteSettingsService.UpdateAsync(Input);
        if (!success)
        {
            ErrorMessage = message;
            return Page();
        }

        SuccessMessage = message;
        return RedirectToPage();
    }
}
