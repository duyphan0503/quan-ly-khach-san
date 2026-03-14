using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Infrastructure.Repositories.Interfaces;
using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IServiceRepository _serviceRepo;

    private readonly AppDbContext _context;
    private readonly IRoomRepository _roomRepo;

    public InvoiceService(
        IInvoiceRepository invoiceRepo,
        IBookingRepository bookingRepo,
        IServiceRepository serviceRepo, 
        AppDbContext context, 
        IRoomRepository roomRepo)
    {
        _invoiceRepo = invoiceRepo;
        _bookingRepo = bookingRepo;
        _serviceRepo = serviceRepo;
        _context = context;
        _roomRepo = roomRepo;
    }

    public Task<List<Invoice>> GetAllAsync() => _invoiceRepo.GetAllAsync();

    public Task<Invoice?> GetByIdAsync(int id) => _invoiceRepo.GetByIdAsync(id);

    public Task<Invoice?> GetByBookingIdAsync(int bookingId) => _invoiceRepo.GetByBookingIdAsync(bookingId);

    public async Task<Invoice> GetOrCreateDraftInvoiceAsync(int bookingId, string? userId = null)
    {
        var existing = await _invoiceRepo.GetByBookingIdAsync(bookingId);
        if (existing is not null) return existing;

        var booking = await _bookingRepo.GetByIdAsync(bookingId);
        if (booking == null) throw new Exception("Không tìm thấy thông tin đặt phòng khi tạo hóa đơn nháp.");

        var invoice = new Invoice
        {
            BookingId = bookingId,
            InvoiceNumber = await _invoiceRepo.GenerateInvoiceNumberAsync(),
            InvoiceDate = DateTime.Now,
            Status = InvoiceStatus.Pending,
            CreatedByUserId = userId
        };

        int nights = (booking.CheckOut - booking.CheckIn).Days;
        if (nights <= 0) nights = 1;

        decimal roomPrice = booking.Room?.RoomType?.BasePrice ?? booking.TotalAmount / Math.Max(1, nights);

        invoice.InvoiceDetails.Add(new InvoiceDetail
        {
            Description = $"Tiền phòng ({nights} đêm)",
            Quantity = nights,
            UnitPrice = roomPrice,
            LineTotal = roomPrice * nights
        });

        invoice.SubTotal = invoice.InvoiceDetails.Sum(d => d.LineTotal);
        invoice.GrandTotal = invoice.SubTotal;

        await _invoiceRepo.AddAsync(invoice);
        return invoice;
    }

    public async Task<(bool Success, string Message)> AddServiceToInvoiceAsync(int bookingId, int serviceId, int quantity)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var draft = await GetOrCreateDraftInvoiceAsync(bookingId);
            if (draft.Status != InvoiceStatus.Pending) return (false, "Hóa đơn đã xử lý, không thể thêm dịch vụ.");

            var svc = await _serviceRepo.GetByIdAsync(serviceId);
            if (svc == null) return (false, "Dịch vụ không tồn tại.");

            var existingDetail = draft.InvoiceDetails.FirstOrDefault(d => d.ServiceId == serviceId);
            if (existingDetail != null)
            {
                existingDetail.Quantity += quantity;
                existingDetail.LineTotal = existingDetail.Quantity * existingDetail.UnitPrice;
            }
            else
            {
                draft.InvoiceDetails.Add(new InvoiceDetail
                {
                    ServiceId = serviceId,
                    Description = svc.Name,
                    Quantity = quantity,
                    UnitPrice = svc.Price,
                    LineTotal = svc.Price * quantity
                });
            }

            draft.SubTotal = draft.InvoiceDetails.Sum(d => d.LineTotal);
            draft.GrandTotal = draft.SubTotal + draft.Tax - draft.Discount;

            await _invoiceRepo.UpdateAsync(draft);
            
            await transaction.CommitAsync();
            
            return (true, $"Đã thêm {quantity}x {svc.Name} vào hóa đơn đặt phòng.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(bool Success, string Message)> RemoveInvoiceDetailAsync(int invoiceId, int detailId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var draft = await _invoiceRepo.GetByIdAsync(invoiceId);
            if (draft == null || draft.Status != InvoiceStatus.Pending)
                return (false, "Không thể xóa chi tiết khỏi hóa đơn này.");

            var detail = draft.InvoiceDetails.FirstOrDefault(d => d.Id == detailId);
            if (detail == null) return (false, "Dịch vụ không tồn tại trong hóa đơn.");

            await _invoiceRepo.DeleteDetailAsync(detail);
                
            draft.InvoiceDetails.Remove(detail);
            draft.SubTotal = draft.InvoiceDetails.Sum(d => d.LineTotal);
            draft.GrandTotal = draft.SubTotal + draft.Tax - draft.Discount;

            await _invoiceRepo.UpdateAsync(draft);
            
            await transaction.CommitAsync();
            return (true, "Đã xóa mục thành công.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(bool Success, string Message, Invoice? Invoice)> FinalizeInvoiceAsync(int bookingId, decimal discount, decimal tax, PaymentMethod method, string? userId = null)
    {
        var result = await FinalizeInvoicesAsync(
            bookingId,
            discount,
            tax,
            method,
            checkoutWholeGroup: false,
            userId);

        return result.Success
            ? (true, result.Message, result.Invoices.FirstOrDefault())
            : (false, result.Message, null);
    }

    public async Task<(bool Success, string Message, List<Invoice> Invoices)> FinalizeInvoicesAsync(
        int bookingId,
        decimal discount,
        decimal tax,
        PaymentMethod method,
        bool checkoutWholeGroup,
        string? userId = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var rootBooking = await _bookingRepo.GetByIdAsync(bookingId);
            if (rootBooking == null)
                return (false, "Không tìm thấy đơn đặt phòng cần thanh toán.", []);

            var targetBookings = new List<Booking> { rootBooking };

            if (checkoutWholeGroup && !string.IsNullOrWhiteSpace(rootBooking.BookingGroupCode))
            {
                var groupBookings = await _bookingRepo.GetByGroupCodeAsync(rootBooking.BookingGroupCode);
                targetBookings = groupBookings
                    .Where(b => b.Status == BookingStatus.CheckedIn)
                    .OrderBy(b => b.Id)
                    .ToList();
            }
            else if (rootBooking.Status != BookingStatus.CheckedIn)
            {
                return (false, "Đơn đặt phòng này chưa ở trạng thái đang lưu trú để checkout.", []);
            }

            if (targetBookings.Count == 0)
                return (false, "Không có phòng nào trong nhóm đang lưu trú để checkout.", []);

            var drafts = new List<Invoice>();
            foreach (var booking in targetBookings)
            {
                var draft = await GetOrCreateDraftInvoiceAsync(booking.Id, userId);
                if (draft.Status != InvoiceStatus.Pending)
                {
                    return (false, $"Hóa đơn của phòng {booking.Room?.RoomNumber} đã thanh toán hoặc bị khóa.", []);
                }

                drafts.Add(draft);
            }

            var totalSubTotal = drafts.Sum(d => d.SubTotal);
            var clampedTax = Math.Max(0, tax);
            var clampedDiscount = Math.Clamp(discount, 0, totalSubTotal + clampedTax);

            var paidInvoices = new List<Invoice>();
            var remainingTax = clampedTax;
            var remainingDiscount = clampedDiscount;

            for (var i = 0; i < drafts.Count; i++)
            {
                var draft = drafts[i];
                var isLast = i == drafts.Count - 1;

                decimal invoiceTax;
                decimal invoiceDiscount;

                if (isLast || totalSubTotal <= 0)
                {
                    invoiceTax = remainingTax;
                    invoiceDiscount = remainingDiscount;
                }
                else
                {
                    var ratio = draft.SubTotal / totalSubTotal;
                    invoiceTax = Math.Round(clampedTax * ratio, 2, MidpointRounding.AwayFromZero);
                    invoiceDiscount = Math.Round(clampedDiscount * ratio, 2, MidpointRounding.AwayFromZero);
                    remainingTax -= invoiceTax;
                    remainingDiscount -= invoiceDiscount;
                }

                draft.Discount = Math.Clamp(invoiceDiscount, 0, draft.SubTotal + invoiceTax);
                draft.Tax = Math.Max(0, invoiceTax);
                draft.PaymentMethod = method;
                draft.GrandTotal = draft.SubTotal + draft.Tax - draft.Discount;
                draft.Status = InvoiceStatus.Paid;
                draft.InvoiceDate = DateTime.Now;

                if (userId != null)
                {
                    draft.CreatedByUserId = userId;
                }

                await _invoiceRepo.UpdateAsync(draft);
                paidInvoices.Add(draft);
            }

            foreach (var booking in targetBookings)
            {
                if (booking.Status != BookingStatus.CheckedOut)
                {
                    booking.Status = BookingStatus.CheckedOut;
                    await _bookingRepo.UpdateAsync(booking);
                }

                var room = await _roomRepo.GetByIdAsync(booking.RoomId);
                if (room != null && room.Status != RoomStatus.Available)
                {
                    room.Status = RoomStatus.Available;
                    await _roomRepo.UpdateAsync(room);
                }
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            if (paidInvoices.Count == 1)
            {
                return (true, "Thanh toán thành công.", paidInvoices);
            }

            return (true, $"Thanh toán thành công {paidInvoices.Count} phòng trong cùng đợt checkout.", paidInvoices);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(bool Success, string Message, Invoice? NewInvoice)> SplitInvoiceAsync(
        int sourceInvoiceId,
        List<int> detailIds,
        string? userId = null)
    {
        if (detailIds == null || detailIds.Count == 0)
        {
            return (false, "Vui lòng chọn ít nhất một dòng chi tiết để tách.", null);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var source = await _invoiceRepo.GetByIdAsync(sourceInvoiceId);
            if (source == null)
            {
                return (false, "Không tìm thấy hóa đơn nguồn.", null);
            }

            if (source.Status != InvoiceStatus.Pending)
            {
                return (false, "Chỉ có thể tách chứng từ khi hóa đơn đang ở trạng thái chờ thanh toán.", null);
            }

            var selectedIds = detailIds.Distinct().ToHashSet();
            var selectedDetails = source.InvoiceDetails
                .Where(d => selectedIds.Contains(d.Id))
                .ToList();

            if (selectedDetails.Count == 0)
            {
                return (false, "Không tìm thấy dòng chi tiết hợp lệ để tách.", null);
            }

            if (selectedDetails.Count >= source.InvoiceDetails.Count)
            {
                return (false, "Không thể tách toàn bộ dòng chi tiết. Vui lòng để lại ít nhất một dòng ở hóa đơn gốc.", null);
            }

            var originalSubTotal = source.SubTotal;
            var movedSubTotal = selectedDetails.Sum(d => d.LineTotal);

            var newInvoice = new Invoice
            {
                BookingId = source.BookingId,
                InvoiceNumber = await _invoiceRepo.GenerateInvoiceNumberAsync(),
                InvoiceDate = DateTime.Now,
                SubTotal = movedSubTotal,
                Tax = 0,
                Discount = 0,
                GrandTotal = movedSubTotal,
                PaymentMethod = source.PaymentMethod,
                Status = InvoiceStatus.Pending,
                CreatedByUserId = userId ?? source.CreatedByUserId
            };

            _context.Invoices.Add(newInvoice);
            await _context.SaveChangesAsync();

            foreach (var detail in selectedDetails)
            {
                detail.InvoiceId = newInvoice.Id;
            }
            _context.InvoiceDetails.UpdateRange(selectedDetails);

            source.SubTotal = Math.Max(0, source.SubTotal - movedSubTotal);

            if (originalSubTotal > 0)
            {
                var ratio = movedSubTotal / originalSubTotal;
                var movedTax = Math.Round(source.Tax * ratio, 2, MidpointRounding.AwayFromZero);
                var movedDiscount = Math.Round(source.Discount * ratio, 2, MidpointRounding.AwayFromZero);

                newInvoice.Tax = movedTax;
                newInvoice.Discount = movedDiscount;
                newInvoice.GrandTotal = newInvoice.SubTotal + newInvoice.Tax - newInvoice.Discount;

                source.Tax = Math.Max(0, source.Tax - movedTax);
                source.Discount = Math.Max(0, source.Discount - movedDiscount);
            }

            source.GrandTotal = source.SubTotal + source.Tax - source.Discount;

            _context.Invoices.Update(source);
            _context.Invoices.Update(newInvoice);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, $"Đã tách {selectedDetails.Count} dòng chi tiết sang hóa đơn #{newInvoice.InvoiceNumber}.", newInvoice);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<decimal> GetTotalRevenueAsync() => _invoiceRepo.GetTotalRevenueAsync();

    public Task<List<decimal>> GetMonthlyRevenueChartAsync(int year) => _invoiceRepo.GetMonthlyRevenueChartAsync(year);
}
