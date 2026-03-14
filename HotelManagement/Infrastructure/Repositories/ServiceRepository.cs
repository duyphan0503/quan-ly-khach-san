using Microsoft.EntityFrameworkCore;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Core.Models;
using HotelManagement.Infrastructure.Repositories.Interfaces;

namespace HotelManagement.Infrastructure.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _context;

    public ServiceRepository(AppDbContext context) => _context = context;

    public async Task<List<Service>> GetAllAsync()
        => await _context.Services.AsNoTracking().ToListAsync();

    public async Task<List<Service>> GetActiveAsync()
        => await _context.Services
            .Where(s => s.IsActive)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Service?> GetByIdAsync(int id)
        => await _context.Services.FindAsync(id);

    public async Task AddAsync(Service service)
    {
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Service service)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
    }

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
