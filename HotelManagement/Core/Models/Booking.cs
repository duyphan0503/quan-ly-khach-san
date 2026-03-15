using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotelManagement.Core.Models.Enums;

namespace HotelManagement.Core.Models;

/// <summary>
/// Đặt phòng — liên kết Guest + Room + ApplicationUser
/// </summary>
public class Booking
{
    // Khóa chính booking.
    public int Id { get; set; }

    [Display(Name = "Khách hàng")]
    // FK trỏ tới Guest.Id.
    public int GuestId { get; set; }

    [Display(Name = "Phòng")]
    // FK trỏ tới Room.Id.
    public int RoomId { get; set; }

    [StringLength(450)]
    // Nhân viên tạo booking (FK về AspNetUsers.Id) - cho phép null khi dữ liệu cũ.
    public string? CreatedByUserId { get; set; }

    [StringLength(50)]
    [Display(Name = "Mã nhóm đặt phòng")]
    // Dùng gom nhiều booking cùng đoàn/công ty để checkout gộp.
    public string? BookingGroupCode { get; set; }

    [Required(ErrorMessage = "Ngày nhận phòng không được để trống")]
    [Display(Name = "Ngày nhận phòng")]
    [DataType(DataType.Date)]
    public DateTime CheckIn { get; set; }

    [Required(ErrorMessage = "Ngày trả phòng không được để trống")]
    [Display(Name = "Ngày trả phòng")]
    [DataType(DataType.Date)]
    public DateTime CheckOut { get; set; }

    [Range(1, 10)]
    [Display(Name = "Số lượng khách")]
    public int NumberOfGuests { get; set; } = 1;

    [Display(Name = "Trạng thái")]
    // Trạng thái nghiệp vụ: Confirmed/CheckedIn/CheckedOut/Cancelled...
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Tổng tiền")]
    // Tổng tiền dự kiến cho booking (thường tính theo số đêm và giá phòng).
    public decimal TotalAmount { get; set; }

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? Notes { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Guest Guest { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public ApplicationUser? CreatedByUser { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    // Giá trị tính toán tại runtime, không lưu DB.
    [NotMapped]
    public int NumberOfNights => (CheckOut - CheckIn).Days;
}
