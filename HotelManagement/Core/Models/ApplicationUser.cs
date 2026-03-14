using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Core.Models;

/// <summary>
/// Mở rộng IdentityUser — thêm trường FullName cho nhân viên
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Ảnh đại diện")]
    public string? AvatarUrl { get; set; }
}
