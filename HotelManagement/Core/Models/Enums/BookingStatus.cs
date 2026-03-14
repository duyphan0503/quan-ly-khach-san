using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Core.Models.Enums;

public enum BookingStatus
{
    [Display(Name = "Chờ duyệt")]
    Pending,

    [Display(Name = "Đã xác nhận")]
    Confirmed,

    [Display(Name = "Đã nhận phòng")]
    CheckedIn,

    [Display(Name = "Đã trả phòng")]
    CheckedOut,

    [Display(Name = "Đã hủy")]
    Cancelled
}
