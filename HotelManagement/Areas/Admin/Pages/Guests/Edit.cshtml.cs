using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Areas.Admin.Pages.Guests;

/// <summary>
/// Xử lý cập nhật thông tin khách, bao gồm upload avatar và lưu thay đổi hồ sơ.
/// </summary>
public class EditModel : PageModel
{
    private readonly IGuestService _guestService;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ khách và môi trường web để lưu file avatar.
    /// </summary>
    public EditModel(IGuestService guestService, IWebHostEnvironment environment)
    {
        _guestService = guestService;
        _environment = environment;
    }

    [BindProperty]
    public EditGuestViewModel Input { get; set; } = new();

    public Guest Guest { get; set; } = new();

    /// <summary>
    /// Nạp hồ sơ khách theo id và map sang view model để hiển thị form chỉnh sửa.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guest = await _guestService.GetByIdAsync(id);
        if (guest == null)
        {
            return NotFound();
        }

        Guest = guest;

        Input = new EditGuestViewModel
        {
            Id = guest.Id,
            FullName = guest.FullName,
            CCCD = guest.CCCD,
            PhoneNumber = guest.PhoneNumber,
            Email = guest.Email,
            Nationality = guest.Nationality,
            Address = guest.Address,
            Gender = guest.Gender,
            DateOfBirth = guest.DateOfBirth,
            AvatarUrl = guest.AvatarUrl
        };

        return Page();
    }

    /// <summary>
    /// Kiểm tra dữ liệu, xử lý avatar mới (nếu có) và cập nhật hồ sơ khách.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Guest = await _guestService.GetByIdAsync(Input.Id) ?? new Guest();
            return Page();
        }

        if (Input.AvatarFile != null)
        {
            try 
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Input.AvatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.AvatarFile.CopyToAsync(fileStream);
                }
                
                Input.AvatarUrl = "/uploads/avatars/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi tải ảnh lên: " + ex.Message);
                return Page();
            }
        }

        var guest = new Guest
        {
            Id = Input.Id,
            FullName = Input.FullName,
            CCCD = Input.CCCD,
            PhoneNumber = Input.PhoneNumber,
            Email = Input.Email,
            Nationality = Input.Nationality ?? "Việt Nam",
            Address = Input.Address,
            Gender = Input.Gender,
            DateOfBirth = Input.DateOfBirth,
            AvatarUrl = Input.AvatarUrl
        };

        var (success, message) = await _guestService.UpdateAsync(guest);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, message);
        Guest = await _guestService.GetByIdAsync(Input.Id) ?? new Guest();
        return Page();
    }

    /// <summary>
    /// Mô hình dữ liệu nhập liệu khi chỉnh sửa hồ sơ khách.
    /// </summary>
    public class EditGuestViewModel
    {
        public int Id { get; set; }

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
        public string? Nationality { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }

        public string? AvatarUrl { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}
