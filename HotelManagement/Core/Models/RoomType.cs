using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

/// <summary>
/// Loại phòng: Standard, Deluxe, Suite, VIP
/// </summary>
public class RoomType
{
    // Khóa chính loại phòng.
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên loại phòng không được để trống")]
    [StringLength(50)]
    [Display(Name = "Loại phòng")]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
    [Display(Name = "Giá cơ bản (VNĐ/đêm)")]
    // Giá niêm yết 1 đêm, chưa bao gồm dịch vụ phát sinh.
    public decimal BasePrice { get; set; }

    [StringLength(500)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Range(1, 10)]
    [Display(Name = "Số khách tối đa")]
    public int MaxOccupancy { get; set; } = 2;

    [StringLength(500)]
    [Display(Name = "Tiện nghi")]
    public string? Amenities { get; set; }

    [StringLength(500)]
    [Display(Name = "Đường dẫn ảnh")]
    public string? ImageUrl { get; set; }

    // Một loại phòng có nhiều phòng vật lý.
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
