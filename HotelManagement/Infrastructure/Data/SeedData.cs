using System.Collections.Frozen;
using Microsoft.AspNetCore.Identity;
using HotelManagement.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Data;

/// <summary>
/// Seed dữ liệu khởi tạo cho môi trường mới: role, user mặc định và dữ liệu demo nền.
/// </summary>
public static class SeedData
{
    // Hàm khởi tạo dữ liệu mẫu tổng: role, user và dữ liệu nghiệp vụ nền.
    /// <summary>
    /// Khởi tạo dữ liệu mặc định ban đầu cho hệ thống.
    /// </summary>
    public static async Task Initialize(
        IServiceProvider serviceProvider,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await EnsureRolesExistAsync(roleManager);

        await EnsureUserExistsAsync(userManager, "manager@hotel.com", "Hotel@123", "Admin Manager", "Manager");
        await EnsureUserExistsAsync(userManager, "le.reception@hotel.com", "Hotel@123", "Lê Thị Hương", "Receptionist");

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedRoomTypesIfEmptyAsync(context);
        await SeedRoomsIfEmptyAsync(context);
        await SeedServicesIfEmptyAsync(context);
        await SeedGuestsIfEmptyAsync(context);
    }

    // Đảm bảo role bắt buộc luôn tồn tại trước khi gán quyền cho tài khoản mặc định.
    private static async Task EnsureRolesExistAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] requiredRoles = ["Manager", "Receptionist"];
        foreach (var roleName in requiredRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    // Tạo tài khoản mặc định nếu chưa tồn tại để tiện login demo/báo cáo.
    private static async Task EnsureUserExistsAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    // Seed loại phòng khi DB chưa có dữ liệu.
    private static async Task SeedRoomTypesIfEmptyAsync(AppDbContext context)
    {
        if (await context.RoomTypes.AnyAsync()) return;

        context.RoomTypes.AddRange([
            new RoomType { Name = "Standard", BasePrice = 500_000m, MaxOccupancy = 2, ImageUrl = "/images/room_standard.jpg", Description = "Phòng tiêu chuẩn thoải mái, phù hợp cho khách du lịch cá nhân hoặc cặp đôi.", Amenities = "WiFi, TV, Điều hòa, Nóng lạnh" },
            new RoomType { Name = "Deluxe", BasePrice = 900_000m, MaxOccupancy = 2, ImageUrl = "/images/room_deluxe.jpg", Description = "Phòng cao cấp với view hồ bơi và ban công thoáng đãng.", Amenities = "WiFi, Smart TV, Điều hòa, Minibar, Tủ lạnh" },
            new RoomType { Name = "Suite", BasePrice = 1_500_000m, MaxOccupancy = 4, ImageUrl = "/images/room_suite.jpg", Description = "Phòng Suite rộng rãi với phòng khách riêng biệt, lý tưởng cho gia đình.", Amenities = "WiFi, Smart TV, Điều hòa, Minibar, Bồn tắm, Phòng khách riêng" },
            new RoomType { Name = "VIP", BasePrice = 3_000_000m, MaxOccupancy = 4, ImageUrl = "/images/room_vip.jpg", Description = "Trải nghiệm đẳng cấp bậc nhất với dịch vụ quản gia riêng và view toàn cảnh thành phố.", Amenities = "WiFi, Smart TV 4K, Điều hòa, Minibar cao cấp, Bồn tắm Jacuzzi, Ban công, Butler" }
        ]);

        await context.SaveChangesAsync();
    }

    // Seed danh sách phòng mẫu dựa trên RoomType đã có.
    private static async Task SeedRoomsIfEmptyAsync(AppDbContext context)
    {
        if (await context.Rooms.AnyAsync()) return;

        // Dùng FrozenDictionary để tra cứu RoomTypeId nhanh và ổn định.
        var roomTypeByName = (await context.RoomTypes.ToListAsync())
            .ToFrozenDictionary(rt => rt.Name, rt => rt.Id);

        context.Rooms.AddRange([
            new Room { RoomNumber = "101", RoomTypeId = roomTypeByName["Standard"], Floor = 1 },
            new Room { RoomNumber = "102", RoomTypeId = roomTypeByName["Standard"], Floor = 1 },
            new Room { RoomNumber = "103", RoomTypeId = roomTypeByName["Standard"], Floor = 1 },
            new Room { RoomNumber = "104", RoomTypeId = roomTypeByName["Deluxe"], Floor = 1 },
            new Room { RoomNumber = "201", RoomTypeId = roomTypeByName["Deluxe"], Floor = 2 },
            new Room { RoomNumber = "202", RoomTypeId = roomTypeByName["Deluxe"], Floor = 2 },
            new Room { RoomNumber = "203", RoomTypeId = roomTypeByName["Suite"], Floor = 2 },
            new Room { RoomNumber = "301", RoomTypeId = roomTypeByName["Suite"], Floor = 3 },
            new Room { RoomNumber = "302", RoomTypeId = roomTypeByName["VIP"], Floor = 3 },
            new Room { RoomNumber = "401", RoomTypeId = roomTypeByName["VIP"], Floor = 4 },
            new Room { RoomNumber = "402", RoomTypeId = roomTypeByName["VIP"], Floor = 4 }
        ]);

        await context.SaveChangesAsync();
    }

    // Seed dịch vụ khách sạn để test luồng invoice/checkout.
    private static async Task SeedServicesIfEmptyAsync(AppDbContext context)
    {
        if (await context.Services.AnyAsync()) return;

        context.Services.AddRange([
            new Service { Name = "Giặt ủi", Price = 50_000m, Unit = "lần" },
            new Service { Name = "Minibar", Price = 100_000m, Unit = "lần" },
            new Service { Name = "Spa & Massage", Price = 300_000m, Unit = "giờ" },
            new Service { Name = "Ăn sáng Buffet", Price = 80_000m, Unit = "phần" },
            new Service { Name = "Xe đưa đón SB", Price = 250_000m, Unit = "lượt" },
            new Service { Name = "Phòng họp", Price = 500_000m, Unit = "giờ" },
            new Service { Name = "Dịch vụ phòng", Price = 30_000m, Unit = "lần" }
        ]);

        await context.SaveChangesAsync();
    }

    // Seed khách hàng mẫu để có dữ liệu minh họa khi demo báo cáo.
    private static async Task SeedGuestsIfEmptyAsync(AppDbContext context)
    {
        if (await context.Guests.AnyAsync()) return;

        context.Guests.AddRange([
            new Guest { FullName = "Nguyễn Văn An", CCCD = "079200001234", PhoneNumber = "0901234567", Email = "an.nguyen@gmail.com", Address = "123 Lê Văn Sỹ, Q.3, TP.HCM", Nationality = "Việt Nam", AvatarUrl = "/uploads/avatars/an.jpg" },
            new Guest { FullName = "Trần Thị Bích", CCCD = "001200005678", PhoneNumber = "0912345678", Email = "bich.tran@outlook.com", Address = "45 Hoàng Diệu, Hà Nội", Nationality = "Việt Nam", AvatarUrl = "/uploads/avatars/bich.jpg" },
            new Guest { FullName = "Lê Minh Cường", CCCD = "048200009012", PhoneNumber = "0923456789", Email = "cuong.le@gmail.com", Address = "89 Nguyễn Văn Linh, Đà Nẵng", Nationality = "Việt Nam", AvatarUrl = "/uploads/avatars/cuong.jpg" },
            new Guest { FullName = "John Smith", CCCD = "A12345678", PhoneNumber = "+1-555-0100", Address = "New York, USA", Nationality = "Mỹ", AvatarUrl = "/uploads/avatars/john.jpg" }
        ]);

        await context.SaveChangesAsync();
    }
}
