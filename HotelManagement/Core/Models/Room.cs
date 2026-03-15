using System.ComponentModel.DataAnnotations;
using HotelManagement.Core.Models.Enums;

namespace HotelManagement.Core.Models;

/// <summary>
/// Phòng khách sạn — liên kết với RoomType
/// </summary>
public class Room
{
    // Khóa chính phòng.
    public int Id { get; set; }

    [Required(ErrorMessage = "Số phòng không được để trống")]
    [StringLength(10)]
    [Display(Name = "Số phòng")]
    public string RoomNumber { get; set; } = string.Empty;

    [Display(Name = "Loại phòng")]
    // FK về RoomType.Id.
    public int RoomTypeId { get; set; }

    [Range(1, 100)]
    [Display(Name = "Tầng")]
    public int Floor { get; set; }

    [Display(Name = "Trạng thái")]
    // Trạng thái vận hành tức thời của phòng.
    public RoomStatus Status { get; set; } = RoomStatus.Available;

    [StringLength(500)]
    [Display(Name = "Ảnh phòng (URL)")]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? Notes { get; set; }

    // Navigation phục vụ Include/join trong EF Core.
    public RoomType RoomType { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
