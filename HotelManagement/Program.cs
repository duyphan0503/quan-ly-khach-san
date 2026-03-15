using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Core.Models;
using HotelManagement.Infrastructure.Repositories;
using HotelManagement.Infrastructure.Repositories.Interfaces;
using HotelManagement.Application.Services;
using HotelManagement.Application.Services.Interfaces;
using System.Globalization;
using HotelManagement.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HotelSettings>(builder.Configuration.GetSection("HotelSettings"));

// Cấu hình DbContext dùng SQL Server và tự retry khi gặp lỗi mạng tạm thời.
ConfigureDatabase(builder.Services, builder.Configuration);

// Cấu hình locale tiếng Việt cho toàn bộ hệ thống.
var supportedCultures = ConfigureVietnameseCulture(builder.Services);

// Cấu hình Identity và thông báo lỗi tiếng Việt.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddErrorDescriber<VietnameseIdentityErrorDescriber>();

// Đăng ký Repository.
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();

// Đăng ký Service nghiệp vụ.
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IWebsiteSettingsService, WebsiteSettingsService>();
builder.Services.AddHostedService<NoShowCancellationWorker>();

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "login");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Logout", "logout");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Register", "register");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/ForgotPassword", "forgot-password");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/ResetPassword", "reset-password");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/Index", "profile");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/Email", "profile/email");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/ChangePassword", "profile/password");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/PersonalData", "profile/data");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/Bookings", "profile/bookings");
});

var app = builder.Build();

// Bắt lỗi chưa xử lý và chuyển hướng về trang lỗi thân thiện.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "UNHANDLED ERROR FOR PATH {Path}", context.Request.Path);
        if (!context.Response.HasStarted)
        {
            context.Response.Redirect("/error");
        }
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Chuẩn hóa xử lý status code: 404 về trang not found, mã khác về trang error.
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == 404)
    {
        context.HttpContext.Request.Path = "/notfound";
        await context.Next(context.HttpContext);
    }
    else
    {
        response.Redirect($"/error?code={response.StatusCode}");
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("vi-VN")
    .AddSupportedCultures("vi-VN")
    .AddSupportedUICultures("vi-VN");
localizationOptions.SupportedCultures = supportedCultures;
localizationOptions.SupportedUICultures = supportedCultures;
app.UseRequestLocalization(localizationOptions);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

await InitializeDatabaseAndSeedAsync(app.Services);

app.Run();

static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(
        configuration.GetConnectionString("DefaultConnection"));
    sqlConnectionStringBuilder.MultipleActiveResultSets = false;
    var sqlConnectionString = sqlConnectionStringBuilder.ConnectionString;

    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(sqlConnectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
}

static CultureInfo[] ConfigureVietnameseCulture(IServiceCollection services)
{
    var appCulture = (CultureInfo)CultureInfo.GetCultureInfo("vi-VN").Clone();
    appCulture.NumberFormat.NumberGroupSeparator = ",";
    appCulture.NumberFormat.CurrencyGroupSeparator = ",";
    appCulture.NumberFormat.CurrencyDecimalDigits = 0;
    appCulture.NumberFormat.CurrencySymbol = "VNĐ";
    appCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
    appCulture.DateTimeFormat.LongDatePattern = "dd/MM/yyyy";
    appCulture.DateTimeFormat.FullDateTimePattern = "dd/MM/yyyy HH:mm:ss";
    appCulture.DateTimeFormat.YearMonthPattern = "MM/yyyy";

    var supportedCultures = new[] { appCulture };
    services.Configure<RequestLocalizationOptions>(options =>
    {
        options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("vi-VN");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
    });

    CultureInfo.DefaultThreadCurrentCulture = appCulture;
    CultureInfo.DefaultThreadCurrentUICulture = appCulture;

    return supportedCultures;
}

static async Task InitializeDatabaseAndSeedAsync(IServiceProvider rootServices)
{
    using var scope = rootServices.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Áp các bản vá dữ liệu/cấu trúc nhỏ để tương thích dữ liệu cũ.
        await ApplyDatabaseCompatibilityPatchesAsync(context);

        // Luôn đảm bảo schema mới nhất trước khi seed.
        await context.Database.MigrateAsync();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedData.Initialize(services, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi khởi tạo database và seed dữ liệu.");
    }
}

static async Task ApplyDatabaseCompatibilityPatchesAsync(AppDbContext context)
{
    var sqlPatches = new[]
    {
        "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Guests]') AND name = 'AvatarUrl') ALTER TABLE [Guests] ADD [AvatarUrl] nvarchar(max) NULL;",
        "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Bookings]') AND name = 'BookingGroupCode') ALTER TABLE [Bookings] ADD [BookingGroupCode] nvarchar(50) NULL;",
        @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'IX_Invoices_BookingId'
                  AND object_id = OBJECT_ID('Invoices')
                  AND is_unique = 1
            )
            DROP INDEX [IX_Invoices_BookingId] ON [Invoices];
        ",
        @"
            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'IX_Invoices_BookingId'
                  AND object_id = OBJECT_ID('Invoices')
            )
            CREATE INDEX [IX_Invoices_BookingId] ON [Invoices]([BookingId]);
        ",
        @"
            UPDATE [Guests]
            SET [AvatarUrl] = '/' + LTRIM(RTRIM([AvatarUrl]))
            WHERE [AvatarUrl] IS NOT NULL
              AND LTRIM(RTRIM([AvatarUrl])) <> ''
              AND [AvatarUrl] NOT LIKE '/%'
              AND [AvatarUrl] NOT LIKE 'http%';
        ",
        @"
            UPDATE [AspNetUsers]
            SET [AvatarUrl] = '/' + LTRIM(RTRIM([AvatarUrl]))
            WHERE [AvatarUrl] IS NOT NULL
              AND LTRIM(RTRIM([AvatarUrl])) <> ''
              AND [AvatarUrl] NOT LIKE '/%'
              AND [AvatarUrl] NOT LIKE 'http%';
        ",
        @"
            UPDATE G
            SET G.AvatarUrl = U.AvatarUrl
            FROM Guests G
            JOIN AspNetUsers U ON G.UserId = U.Id
            WHERE U.AvatarUrl IS NOT NULL
              AND (G.AvatarUrl IS NULL OR G.AvatarUrl != U.AvatarUrl);
        ",
        @"
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_BookingGroupCode' AND object_id = OBJECT_ID('Bookings'))
            CREATE INDEX [IX_Bookings_BookingGroupCode] ON [Bookings]([BookingGroupCode]) WHERE [BookingGroupCode] IS NOT NULL;
        ",
        @"
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Guests_Phone' AND object_id = OBJECT_ID('Guests'))
            AND NOT EXISTS (
                SELECT [Phone]
                FROM [Guests]
                WHERE [Phone] IS NOT NULL
                GROUP BY [Phone]
                HAVING COUNT(*) > 1
            )
            CREATE UNIQUE INDEX [IX_Guests_Phone] ON [Guests]([Phone]);
        ",
        @"
            UPDATE G
            SET G.UserId = U.Id
            FROM Guests G
            JOIN AspNetUsers U ON G.[Phone] = U.PhoneNumber
            WHERE G.UserId IS NULL
              AND G.[Phone] IS NOT NULL
              AND U.PhoneNumber IS NOT NULL;
        ",
        @"
            UPDATE G
            SET G.AvatarUrl = U.AvatarUrl
            FROM Guests G
            JOIN AspNetUsers U ON G.UserId = U.Id
            WHERE U.AvatarUrl IS NOT NULL
              AND (G.AvatarUrl IS NULL OR G.AvatarUrl != U.AvatarUrl);
        ",
        "UPDATE Guests SET AvatarUrl = '/uploads/avatars/an.jpg' WHERE FullName = N'Nguyễn Văn An' AND (AvatarUrl IS NULL OR AvatarUrl = 'uploads/avatars/an.jpg' OR AvatarUrl = '/uploads/avatars/an.jpg');",
        "UPDATE Guests SET AvatarUrl = '/uploads/avatars/bich.jpg' WHERE FullName = N'Trần Thị Bích' AND (AvatarUrl IS NULL OR AvatarUrl = 'uploads/avatars/bich.jpg' OR AvatarUrl = '/uploads/avatars/bich.jpg');",
        "UPDATE Guests SET AvatarUrl = '/uploads/avatars/cuong.jpg' WHERE FullName = N'Lê Minh Cường' AND (AvatarUrl IS NULL OR AvatarUrl = 'uploads/avatars/cuong.jpg' OR AvatarUrl = '/uploads/avatars/cuong.jpg');",
        "UPDATE Guests SET AvatarUrl = '/uploads/avatars/john.jpg' WHERE FullName = 'John Smith' AND (AvatarUrl IS NULL OR AvatarUrl = 'uploads/avatars/john.jpg' OR AvatarUrl = '/uploads/avatars/john.jpg');"
    };

    foreach (var sql in sqlPatches)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
        }
        catch
        {
            // Bỏ qua bản vá lỗi để app vẫn có thể chạy, chi tiết xem log migration/DB.
        }
    }
}
