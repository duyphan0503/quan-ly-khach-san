using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Core.Models;

namespace HotelManagement.Infrastructure.Data;

/// <summary>
/// DbContext trung tâm: quản lý mapping EF Core và ràng buộc dữ liệu toàn hệ thống.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Khởi tạo lớp AppDbContext và nạp các dependency cần thiết.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceDetail> InvoiceDetails => Set<InvoiceDetail>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Luôn gọi base để Identity tự cấu hình đầy đủ bảng AspNetUsers/Roles/... trước.
        base.OnModelCreating(builder);

        // ── Room ──
        builder.Entity<Room>()
            .HasIndex(r => r.RoomNumber)
            .IsUnique();
        builder.Entity<Room>()
            .HasOne(r => r.RoomType)
            .WithMany(rt => rt.Rooms)
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Guest ──
        builder.Entity<Guest>()
            .HasIndex(g => g.CCCD)
            .IsUnique()
            .HasFilter("[CCCD] IS NOT NULL"); // NULL không tính là duplicate

        builder.Entity<Guest>()
            .HasIndex(g => g.PhoneNumber)
            .IsUnique();

        // ── Booking ──
        builder.Entity<Booking>()
            .HasOne(b => b.Guest)
            .WithMany(g => g.Bookings)
            .HasForeignKey(b => b.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.CreatedByUser)
            .WithMany()
            .HasForeignKey(b => b.CreatedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Booking>()
            .HasIndex(b => b.BookingGroupCode)
            .HasFilter("[BookingGroupCode] IS NOT NULL");

        // Check constraint bảo vệ dữ liệu ở tầng DB (không phụ thuộc UI/Service).
        builder.Entity<Booking>().ToTable(t => t.HasCheckConstraint("CK_Booking_Dates", "[CheckOut] > [CheckIn]"));

        // ── Invoice ──
        builder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        builder.Entity<Invoice>()
            .HasIndex(i => i.BookingId);

        builder.Entity<Invoice>()
            .HasOne(i => i.Booking)
            .WithMany(b => b.Invoices)
            .HasForeignKey(i => i.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(i => i.CreatedByUser)
            .WithMany()
            .HasForeignKey(i => i.CreatedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ── InvoiceDetail ──
        builder.Entity<InvoiceDetail>()
            .HasOne(d => d.Invoice)
            .WithMany(i => i.InvoiceDetails)
            .HasForeignKey(d => d.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InvoiceDetail>()
            .HasOne(d => d.Service)
            .WithMany(s => s.InvoiceDetails)
            .HasForeignKey(d => d.ServiceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
