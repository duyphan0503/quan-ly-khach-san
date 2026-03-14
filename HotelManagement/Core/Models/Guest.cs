using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Core.Models;

/// <summary>
/// Khách hàng lưu trú tại khách sạn
/// </summary>
public class Guest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Họ tên không được để trống")]
    [StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "CCCD/Hộ chiếu")]
    public string? CCCD { get; set; }

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [StringLength(20)]
    [Phone]
    [Display(Name = "Số điện thoại")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Phone")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(100)]
    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(200)]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [StringLength(50)]
    [Display(Name = "Quốc tịch")]
    public string Nationality { get; set; } = "Việt Nam";

    [Display(Name = "Ảnh đại diện")]
    public string? AvatarUrl { get; set; }

    [StringLength(10)]
    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }

    [Display(Name = "Ngày sinh")]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Mã người dùng Identity")]
    public string? UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
