using Microsoft.EntityFrameworkCore;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Core.Models;
using HotelManagement.Infrastructure.Repositories.Interfaces;

namespace HotelManagement.Infrastructure.Repositories;

/// <summary>
/// Thực thi truy cập dữ liệu cho miền service.
/// </summary>
public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Khởi tạo lớp ServiceRepository và nạp các dependency cần thiết.
    /// </summary>
    public ServiceRepository(AppDbContext context) => _context = context;

    /// <summary>
    /// Lấy toàn bộ dữ liệu service.
    /// </summary>
    public async Task<List<Service>> GetAllAsync()
        => await _context.Services.AsNoTracking().ToListAsync();

    /// <summary>
    /// Thực hiện nghiệp vụ của miền service.
    /// </summary>
    public async Task<List<Service>> GetActiveAsync()
        => await _context.Services
            .Where(s => s.IsActive)
            .AsNoTracking()
            .ToListAsync();

    /// <summary>
    /// Lấy thông tin service theo mã định danh.
    /// </summary>
    public async Task<Service?> GetByIdAsync(int id)
        => await _context.Services.FindAsync(id);

    /// <summary>
    /// Tạo mới dữ liệu service.
    /// </summary>
    public async Task AddAsync(Service service)
    {
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Cập nhật thông tin dữ liệu service.
    /// </summary>
    public async Task UpdateAsync(Service service)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Xóa dữ liệu service theo tiêu chí được truyền vào.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service is not null)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }
    }
}

