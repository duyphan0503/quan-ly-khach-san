# Minh Quang Luxury Hotel Management System

Hệ thống quản lý khách sạn xây dựng bằng ASP.NET Core Razor Pages (.NET 10), phục vụ 2 luồng chính:

- vận hành nội bộ (Admin: lễ tân, quản lý),
- cổng công khai (Public: khách xem phòng, gửi yêu cầu đặt phòng).

## 1) Tính năng chính

- Dashboard KPI: doanh thu, công suất, booking gần đây, tình trạng phòng.
- Quản lý RoomType / Room / Guest / Service.
- Quản lý booking theo vòng đời: tạo → check-in → check-out.
- Quản lý invoice và chi tiết dịch vụ phát sinh.
- Identity + phân quyền vai trò `Manager` và `Receptionist`.
- Worker nền tự động xử lý no-show: `NoShowCancellationWorker`.

## 2) Tech stack

- .NET 10, ASP.NET Core Razor Pages
- Entity Framework Core 10 + SQL Server
- ASP.NET Core Identity
- ChartJSCore
- SixLabors.ImageSharp

## 3) Kiến trúc dự án

Monolith theo hướng N-Tier:

- `Core`: model/domain/constants
- `Infrastructure`: DbContext, repository, identity integration, seed
- `Application`: service nghiệp vụ
- `Areas` (UI): Razor Pages cho `Admin`, `Public`, `Identity`

Luồng chuẩn:

`PageModel -> Application Service -> Repository -> AppDbContext -> SQL Server`

## 4) Cấu trúc thư mục hiện tại

```text
quan-ly-khach-san/
├── HotelManagement/
│   ├── Application/
│   ├── Areas/
│   ├── Core/
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   └── SeedData.cs
│   │   └── Repositories/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   └── HotelManagement.csproj
├── BI_KIP_GIAI_TRINH.md
├── README.md
└── quan-ly-khach-san.sln
```

## 5) Yêu cầu môi trường

- Windows 10/11 hoặc Windows Server
- .NET SDK 10+
- SQL Server (Express/Developer/Standard đều được)

## 6) Chạy nhanh local

### Bước 1: Restore tool và package

```bash
dotnet tool restore
dotnet restore
```

### Bước 2: Cấu hình connection string

Mặc định trong `HotelManagement/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=HotelManagementDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=False"
}
```

Nếu dùng SQL Server instance khác, cập nhật lại giá trị `Server` cho phù hợp.

### Bước 3: Run ứng dụng

```bash
dotnet run --project HotelManagement
```

`launchSettings.json` đã cố định profile local ở `http://localhost:5037`.

### Bước 4: Truy cập

- Public: `http://localhost:5037/`
- Admin: `http://localhost:5037/admin`
- Login: `http://localhost:5037/login`

## 7) Database bootstrap thực tế khi startup

Trong `Program.cs`, ứng dụng chạy theo thứ tự:

1. `context.Database.MigrateAsync()`
2. `SeedData.Initialize(...)`

Nghĩa là:

- schema được áp dụng từ EF migrations,
- dữ liệu mẫu được seed bằng code C# (không dùng script SQL thủ công).

## 8) Seed data và tài khoản mặc định

`SeedData.cs` seed idempotent các nhóm dữ liệu:

- role: `Manager`, `Receptionist`
- user mặc định
- room types, rooms, services, guests

Tài khoản mặc định:

- `manager@hotel.com` / `Hotel@123`
- `le.reception@hotel.com` / `Hotel@123`

> Khuyến nghị: đổi mật khẩu ngay khi triển khai thật.

## 9) EF Core migrations

Tool `dotnet-ef` đã khai báo trong `dotnet-tools.json`.

Lệnh thường dùng:

```bash
dotnet ef migrations add <MigrationName> --project HotelManagement --startup-project HotelManagement
dotnet ef database update --project HotelManagement --startup-project HotelManagement
dotnet ef migrations list --project HotelManagement --startup-project HotelManagement
```

Nếu máy clone mới chưa có thư mục migrations trong working tree, hãy tạo migration đầu tiên trước khi chạy chính thức.

## 10) Cấu hình quan trọng

Trong `appsettings.json`:

- `HotelSettings:NoShowThresholdHours`: ngưỡng xử lý no-show
- `HotelSettings:CheckInTime`, `HotelSettings:CheckOutTime`
- `WebsiteSettings:*`: thông tin branding và liên hệ website

Trong `Program.cs`:

- locale hệ thống chuẩn `vi-VN`
- format tiền tệ VNĐ, không chữ số thập phân
- định dạng ngày `dd/MM/yyyy`

## 11) Build, kiểm thử, publish

```bash
dotnet build
dotnet test
dotnet publish HotelManagement/HotelManagement.csproj -c Release -o ./publish
```

> Hiện solution chưa tách test project riêng, nên `dotnet test` có thể không chạy test case nào.

## 12) Troubleshooting nhanh

### Không kết nối SQL Server

- Kiểm tra SQL Server service đang chạy.
- Kiểm tra lại `ConnectionStrings:DefaultConnection`.
- Kiểm tra quyền truy cập DB của tài khoản chạy app.

### Lỗi migration / schema chưa đúng

```bash
dotnet ef migrations list --project HotelManagement --startup-project HotelManagement
dotnet ef database update --project HotelManagement --startup-project HotelManagement
```

### Build warning NU1900/NU190x do mạng

Nếu môi trường chặn truy cập metadata vulnerability từ NuGet, có thể thấy warning liên quan audit package. Đây thường là vấn đề mạng/repository feed.

## 13) Ghi chú vận hành

- App đã cấu hình route lowercase và alias route cho Identity (`/login`, `/logout`, `/profile`, ...).
- Có middleware bắt lỗi chưa xử lý và status code để chuyển hướng trang lỗi thân thiện.
- Có worker nền no-show chạy theo HostedService.
