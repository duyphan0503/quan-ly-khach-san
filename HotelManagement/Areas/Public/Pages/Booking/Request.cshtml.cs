using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Application.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace HotelManagement.Areas.Public.Pages.Booking;

public class RequestModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IGuestService _guestService;
    private readonly IBookingService _bookingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RequestModel(
        IRoomService roomService, 
        IGuestService guestService, 
        IBookingService bookingService,
        UserManager<ApplicationUser> userManager)
    {
        _roomService = roomService;
        _guestService = guestService;
        _bookingService = bookingService;
        _userManager = userManager;
    }

    [BindProperty]
    public BookingRequestForm Input { get; set; } = new();

    public List<Room> AvailableRooms { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? roomId, DateTime? checkIn, DateTime? checkOut, int? guests)
    {
        Input.CheckIn = checkIn ?? DateTime.Today;
        Input.CheckOut = checkOut ?? DateTime.Today.AddDays(1);
        Input.NumberOfGuests = guests ?? 2;
        await LoadSelectLists(Input.CheckIn, Input.CheckOut);
        
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            Input.FullName = user.FullName;
            Input.Email = user.Email ?? string.Empty;
            Input.Phone = user.PhoneNumber ?? string.Empty;
        }

        if (roomId.HasValue)
        {
            var room = await _roomService.GetByIdAsync(roomId.Value);
            if (room != null)
            {
                Input.SelectedRoomIds = new List<int> { room.Id };
            }
        }
        
        return Page();
    }

    public async Task<PartialViewResult> OnGetRoomListAsync(DateTime? checkIn, DateTime? checkOut, string? selectedRoomIds)
    {
        var effectiveCheckIn = checkIn ?? DateTime.Today;
        var effectiveCheckOut = checkOut ?? effectiveCheckIn.AddDays(1);
        if (effectiveCheckOut <= effectiveCheckIn)
        {
            effectiveCheckOut = effectiveCheckIn.AddDays(1);
        }

        var selectedIds = (selectedRoomIds ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(raw => int.TryParse(raw, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var rooms = await GetAvailableRoomsAsync(effectiveCheckIn, effectiveCheckOut);
        var model = new RoomListViewModel
        {
            AvailableRooms = rooms,
            SelectedRoomIds = selectedIds
        };

        return Partial("_RoomList", model);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists(Input.CheckIn, Input.CheckOut);
            return Page();
        }

        if (Input.CheckOut <= Input.CheckIn)
        {
            ModelState.AddModelError("Input.CheckOut", "Ngày trả phòng phải sau ngày nhận phòng.");
            await LoadSelectLists(Input.CheckIn, Input.CheckOut);
            return Page();
        }

        if (Input.SelectedRoomIds == null || Input.SelectedRoomIds.Count == 0)
        {
            ModelState.AddModelError("Input.SelectedRoomIds", "Vui lòng chọn ít nhất một phòng.");
            await LoadSelectLists(Input.CheckIn, Input.CheckOut);
            return Page();
        }

        var distinctRoomIds = Input.SelectedRoomIds.Distinct().ToList();
        var allRooms = await _roomService.GetAllAsync();
        var selectedRooms = allRooms
            .Where(r => distinctRoomIds.Contains(r.Id))
            .ToList();

        if (selectedRooms.Count != distinctRoomIds.Count)
        {
            ModelState.AddModelError("Input.SelectedRoomIds", "Một số phòng đã chọn không tồn tại hoặc không hợp lệ.");
            await LoadSelectLists(Input.CheckIn, Input.CheckOut);
            return Page();
        }

        foreach (var room in selectedRooms)
        {
            var isAvailable = await _bookingService.IsRoomAvailableAsync(room.Id, Input.CheckIn, Input.CheckOut);
            if (!isAvailable)
            {
                ModelState.AddModelError(string.Empty, $"Xin lỗi! Phòng {room.RoomNumber} đã được đặt trong khoảng thời gian này. Vui lòng chọn phòng khác.");
                await LoadSelectLists(Input.CheckIn, Input.CheckOut);
                return Page();
            }
        }

        var totalCapacity = selectedRooms.Sum(r => r.RoomType.MaxOccupancy);
        if (totalCapacity < Input.NumberOfGuests)
        {
            ModelState.AddModelError(
                "Input.SelectedRoomIds",
                $"Tổng sức chứa các phòng đã chọn ({totalCapacity} khách) không đủ cho {Input.NumberOfGuests} khách. Vui lòng chọn thêm phòng.");
            await LoadSelectLists(Input.CheckIn, Input.CheckOut);
            return Page();
        }

        var normalizedPhone = NormalizePhone(Input.Phone);
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            normalizedPhone = Input.Phone.Trim();
        }
        var normalizedCccd = string.IsNullOrWhiteSpace(Input.CCCD) ? null : Input.CCCD.Trim();
        var currentUser = await _userManager.GetUserAsync(User);

        // Ưu tiên hồ sơ theo tài khoản đăng nhập hiện tại để cho phép cùng 1 tài khoản đặt nhiều lần.
        Guest? guest = null;
        Guest? phoneGuest = null;
        List<Guest> currentUserGuests = new();
        if (currentUser is not null)
        {
            var consolidateResult = await _guestService.ConsolidateGuestProfilesAsync(
                currentUser.Id,
                normalizedPhone,
                normalizedCccd,
                Input.Email,
                Input.FullName,
                currentUser.AvatarUrl);
            if (!consolidateResult.Success)
            {
                ModelState.AddModelError(string.Empty, consolidateResult.Message);
                await LoadSelectLists(Input.CheckIn, Input.CheckOut);
                return Page();
            }

            guest = consolidateResult.PrimaryGuest;
            currentUserGuests = await _guestService.GetAllByUserIdAsync(currentUser.Id);
            guest ??= currentUserGuests
                .FirstOrDefault(g => NormalizePhone(g.PhoneNumber) == normalizedPhone);

            if (guest is null && !string.IsNullOrWhiteSpace(normalizedCccd))
            {
                guest = currentUserGuests
                    .FirstOrDefault(g => !string.IsNullOrWhiteSpace(g.CCCD) &&
                        string.Equals(g.CCCD, normalizedCccd, StringComparison.OrdinalIgnoreCase));
            }

            guest ??= currentUserGuests.FirstOrDefault();
            phoneGuest = await _guestService.SearchByPhoneOrCCCDAsync(normalizedPhone);

            if (phoneGuest is not null &&
                guest is not null &&
                phoneGuest.Id != guest.Id)
            {
                if (!string.IsNullOrWhiteSpace(phoneGuest.UserId) &&
                    !string.Equals(phoneGuest.UserId, currentUser.Id, StringComparison.Ordinal))
                {
                    ModelState.AddModelError(string.Empty, "Số điện thoại này đang liên kết với một tài khoản khác.");
                    await LoadSelectLists(Input.CheckIn, Input.CheckOut);
                    return Page();
                }

                // Ưu tiên hồ sơ đúng theo SĐT để tránh va chạm unique khi hồ sơ UserId cũ/lệch dữ liệu.
                guest = phoneGuest;
            }
        }

        // Nếu chưa có hồ sơ theo UserId thì mới fallback theo SĐT/CCCD.
        if (guest is null)
        {
            guest = phoneGuest ?? await _guestService.SearchByPhoneOrCCCDAsync(normalizedPhone);
            if (guest is null && !string.IsNullOrWhiteSpace(normalizedCccd))
            {
                guest = await _guestService.SearchByPhoneOrCCCDAsync(normalizedCccd);
            }
        }

        // Tránh sửa nhầm hồ sơ thuộc tài khoản khác.
        if (guest is not null &&
            currentUser is not null &&
            !string.IsNullOrWhiteSpace(guest.UserId) &&
            !string.Equals(guest.UserId, currentUser.Id, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Số điện thoại/CCCD này đang liên kết với một tài khoản khác.");
            await LoadSelectLists(Input.CheckIn, Input.CheckOut);
            return Page();
        }

        (bool Success, string Message) guestResult;
        if (guest is null)
        {
            guest = new Guest
            {
                FullName = Input.FullName.Trim(),
                Email = Input.Email.Trim(),
                PhoneNumber = normalizedPhone,
                CCCD = normalizedCccd,
                Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim(),
                UserId = currentUser?.Id,
                AvatarUrl = currentUser?.AvatarUrl
            };

            guestResult = await _guestService.CreateAsync(guest);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(normalizedCccd) &&
                !string.IsNullOrWhiteSpace(guest.CCCD) &&
                !string.Equals(guest.CCCD, normalizedCccd, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Thông tin CCCD không khớp với hồ sơ khách đã tồn tại.");
                await LoadSelectLists(Input.CheckIn, Input.CheckOut);
                return Page();
            }

            guest.FullName = Input.FullName.Trim();
            guest.Email = Input.Email.Trim();
            guest.PhoneNumber = normalizedPhone;
            guest.CCCD = normalizedCccd ?? guest.CCCD;
            guest.Address = string.IsNullOrWhiteSpace(Input.Address) ? guest.Address : Input.Address.Trim();
            guest.UserId ??= currentUser?.Id;
            if (string.IsNullOrWhiteSpace(guest.AvatarUrl) && !string.IsNullOrWhiteSpace(currentUser?.AvatarUrl))
            {
                guest.AvatarUrl = currentUser.AvatarUrl;
            }

            guestResult = await _guestService.UpdateAsync(guest);
        }

        if (!guestResult.Success &&
            currentUser is not null &&
            guestResult.Message.Contains("Số điện thoại", StringComparison.OrdinalIgnoreCase))
        {
            var fallbackGuest = await _guestService.SearchByPhoneOrCCCDAsync(normalizedPhone);
            if (fallbackGuest is not null &&
                (string.IsNullOrWhiteSpace(fallbackGuest.UserId) ||
                 string.Equals(fallbackGuest.UserId, currentUser.Id, StringComparison.Ordinal)))
            {
                fallbackGuest.FullName = Input.FullName.Trim();
                fallbackGuest.Email = Input.Email.Trim();
                fallbackGuest.PhoneNumber = normalizedPhone;
                fallbackGuest.CCCD = normalizedCccd ?? fallbackGuest.CCCD;
                fallbackGuest.Address = string.IsNullOrWhiteSpace(Input.Address) ? fallbackGuest.Address : Input.Address.Trim();
                fallbackGuest.UserId ??= currentUser.Id;

                guestResult = await _guestService.UpdateAsync(fallbackGuest);
                guest = fallbackGuest;
            }
        }

        if (!guestResult.Success)
        {
            ModelState.AddModelError(string.Empty, guestResult.Message);
            await LoadSelectLists(Input.CheckIn, Input.CheckOut);
            return Page();
        }

        // Luồng cũ với 1 phòng: giữ trang xác nhận hiện tại.
        if (distinctRoomIds.Count == 1)
        {
            var booking = new HotelManagement.Core.Models.Booking
            {
                GuestId = guest.Id,
                RoomId = distinctRoomIds[0],
                CheckIn = Input.CheckIn,
                CheckOut = Input.CheckOut,
                NumberOfGuests = Input.NumberOfGuests,
                Status = BookingStatus.Pending,
                Notes = Input.Notes
            };

            var bookingResult = await _bookingService.CreateAsync(booking);
            if (!bookingResult.Success)
            {
                ModelState.AddModelError(string.Empty, bookingResult.Message);
                await LoadSelectLists(Input.CheckIn, Input.CheckOut);
                return Page();
            }

            await _bookingService.UpdateStatusAsync(booking.Id, nameof(BookingStatus.Pending));
            return RedirectToPage("./Confirmation", new { id = booking.Id });
        }

        // Luồng nhóm nhiều phòng.
        var groupedBookingResult = await _bookingService.CreateMultipleAsync(
            guest.Id,
            distinctRoomIds,
            Input.CheckIn,
            Input.CheckOut,
            Input.NumberOfGuests,
            BookingStatus.Pending,
            Input.Notes,
            currentUser?.Id);
        if (!groupedBookingResult.Success)
        {
            ModelState.AddModelError(string.Empty, groupedBookingResult.Message);
            await LoadSelectLists(Input.CheckIn, Input.CheckOut);
            return Page();
        }

        if (groupedBookingResult.PrimaryBookingId.HasValue)
        {
            TempData["StatusMessage"] = groupedBookingResult.Message;
            return RedirectToPage("./Confirmation", new { id = groupedBookingResult.PrimaryBookingId.Value });
        }

        TempData["ErrorMessage"] = "Không thể tạo trang xác nhận cho đơn đặt phòng nhóm.";
        return RedirectToPage("/Index", new { area = "Public" });
    }

    private static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("84", StringComparison.Ordinal) && digits.Length == 11)
        {
            return $"0{digits[2..]}";
        }

        return digits;
    }

    private async Task LoadSelectLists(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut <= checkIn)
        {
            checkOut = checkIn.AddDays(1);
        }

        var availableByDate = await GetAvailableRoomsAsync(checkIn, checkOut);

        AvailableRooms = availableByDate
            .OrderBy(r => r.RoomType.Name)
            .ThenBy(r => r.RoomNumber)
            .ToList();
    }

    private async Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
    {
        var rooms = await _roomService.GetAllAsync();
        var availableByDate = new List<Room>();
        foreach (var room in rooms.Where(r => r.Status == RoomStatus.Available))
        {
            var isAvailable = await _bookingService.IsRoomAvailableAsync(room.Id, checkIn, checkOut);
            if (isAvailable)
            {
                availableByDate.Add(room);
            }
        }

        return availableByDate;
    }
}

public class BookingRequestForm
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;
    
    [Display(Name = "CCCD/Passport")]
    public string? CCCD { get; set; }
    
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [Display(Name = "Phòng đã chọn")]
    public List<int> SelectedRoomIds { get; set; } = new();

    [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    [Display(Name = "Ngày nhận phòng")]
    public DateTime CheckIn { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    [Display(Name = "Ngày trả phòng")]
    public DateTime CheckOut { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số người")]
    [Range(1, 10, ErrorMessage = "Số người từ 1 đến 10")]
    [Display(Name = "Số người")]
    public int NumberOfGuests { get; set; } = 2;

    [Display(Name = "Yêu cầu đặc biệt")]
    public string? Notes { get; set; }
}

public class RoomListViewModel
{
    public List<Room> AvailableRooms { get; set; } = new();
    public List<int> SelectedRoomIds { get; set; } = new();
}
