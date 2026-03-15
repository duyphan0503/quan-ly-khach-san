using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Areas.Admin.Pages.Guests;

/// <summary>
/// Xử lý tạo mới hồ sơ khách lưu trú từ form quản trị.
/// </summary>
public class CreateModel : PageModel
{
    private readonly IGuestService _guestService;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ quản lý khách.
    /// </summary>
    public CreateModel(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [BindProperty]
    public CreateGuestViewModel Input { get; set; } = new();

    /// <summary>
    /// Hiển thị form tạo khách.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Kiểm tra dữ liệu đầu vào và tạo hồ sơ khách mới.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var guest = new Guest
        {
            FullName = Input.FullName,
            CCCD = Input.CCCD,
            PhoneNumber = Input.PhoneNumber,
            Email = Input.Email,
            Nationality = Input.Nationality ?? "Vietnamese",
            Address = Input.Address,
            Gender = Input.Gender,
            DateOfBirth = Input.DateOfBirth
        };

        var (success, message) = await _guestService.CreateAsync(guest);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, message);
        return Page();
    }

    /// <summary>
    /// Mô hình dữ liệu nhập liệu khi tạo khách.
    /// </summary>
    public class CreateGuestViewModel
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "CCCD/Hộ chiếu")]
        public string? CCCD { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Quốc tịch")]
        public string? Nationality { get; set; } = "Việt Nam";

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; } = "Khác";

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }
    }
}
