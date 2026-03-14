# Minh Quang Luxury Hotel Management System

Hệ thống quản lý khách sạn cao cấp cho thị trường Việt Nam, xây dựng bằng ASP.NET Core Razor Pages (.NET 10), tập trung vào vận hành lễ tân - quản trị nội bộ - cổng đặt phòng công khai.

## Mục tiêu chính

- Quản lý phòng, loại phòng, khách hàng, đặt phòng, dịch vụ, hóa đơn trong một hệ thống thống nhất.
- Giao diện dark-mode theo phong cách "Liquid Glass", tối ưu trải nghiệm cho nghiệp vụ khách sạn.
- 100% nội dung tiếng Việt, chuẩn hóa định dạng tiền tệ và ngày tháng theo `vi-VN`.
- Kiến trúc N-Tier rõ ràng để dễ mở rộng và bảo trì.

## Tính năng nổi bật

- Dashboard quản trị với KPI doanh thu, công suất phòng, khách hàng và biểu đồ vận hành.
- Quản lý vòng đời booking: tạo, cập nhật, check-in/check-out, tự động hủy no-show theo ngưỡng giờ.
- Quản lý hóa đơn và checkout nhóm phòng.
- Quản lý hồ sơ khách hàng (bao gồm avatar, lịch sử hoạt động).
- Cổng Public cho khách xem phòng và gửi yêu cầu đặt phòng.
- Xác thực và phân quyền bằng ASP.NET Core Identity (`Manager`, `Receptionist`).

## Mục lục

- [1. Tech Stack](#1-tech-stack)
- [2. Kiến trúc hệ thống](#2-kiến-trúc-hệ-thống)
- [3. Cấu trúc thư mục](#3-cấu-trúc-thư-mục)
- [4. Yêu cầu cài đặt](#4-yêu-cầu-cài-đặt)
- [5. Triển khai nhanh cho khách hàng](#5-triển-khai-nhanh-cho-khách-hàng)
- [6. Cấu hình môi trường](#6-cấu-hình-môi-trường)
- [7. Tài khoản mặc định sau seed](#7-tài-khoản-mặc-định-sau-seed)
- [8. Cơ sở dữ liệu & migration](#8-cơ-sở-dữ-liệu--migration)
- [9. Scripts & lệnh thường dùng](#9-scripts--lệnh-thường-dùng)
- [10. Luồng nghiệp vụ chính](#10-luồng-nghiệp-vụ-chính)
- [11. Testing & quality checks](#11-testing--quality-checks)
- [12. Deployment](#12-deployment)
- [13. Troubleshooting](#13-troubleshooting)
- [14. Ghi chú quan trọng](#14-ghi-chú-quan-trọng)

## 1. Tech Stack

- **Backend**: ASP.NET Core Razor Pages (.NET 10)
- **ORM**: Entity Framework Core 10 (SQL Server provider)
- **Database**: SQL Server 2025
- **Auth**: ASP.NET Core Identity
- **Charts**: Chart.js + ChartJSCore
- **Image handling**: SixLabors.ImageSharp
- **UI**:
  - Tailwind CSS (CDN)
  - DaisyUI (CDN)
  - Flowbite (CDN, khu vực Admin)
  - Iconify
- **Background processing**: `BackgroundService` (`NoShowCancellationWorker`)

## 2. Kiến trúc hệ thống

Dự án theo N-Tier trong một monolith:

- **Core**: Domain models, enums, constants.
- **Infrastructure**: `AppDbContext`, migrations, repositories, identity integration.
- **Application**: Services, business rules, ViewModels.
- **UI (Areas)**: Razor Pages cho `Admin`, `Public`, `Identity`.

Luồng chuẩn:

`Razor Page/PageModel -> Service -> Repository -> DbContext -> SQL Server`

Các nguyên tắc chính:

- PageModel chỉ bind dữ liệu + gọi service.
- Business logic nằm ở service.
- Repository chỉ xử lý persistence/query.
- Mutation service trả tuple dạng `Task<(bool Success, string Message)>` (hoặc biến thể mở rộng).

## 3. Cấu trúc thư mục

```text
quan-ly-khach-san/
├── HotelManagement/
│   ├── Application/
│   │   ├── Services/
│   │   └── ViewModels/
│   ├── Areas/
│   │   ├── Admin/Pages/
│   │   ├── Public/Pages/
│   │   └── Identity/Pages/
│   ├── Core/
│   │   ├── Constants/
│   │   └── Models/
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── Migrations/
│   │   │   └── SeedData.cs
│   │   ├── Identity/
│   │   └── Repositories/
│   ├── wwwroot/
│   ├── Program.cs
│   ├── appsettings.json
│   └── HotelManagement.csproj
└── quan-ly-khach-san.sln
```

## 4. Yêu cầu cài đặt

- .NET SDK 10.0+
- SQL Server 2022/2025
- SQL client tùy chọn (`sqlcmd`, Azure Data Studio, DBeaver)
- OS: Windows Server/Windows 10+

## 5. Triển khai nhanh cho khách hàng

### Bước 1: Clone source

```bash
git clone <REPO_URL>
cd quan-ly-khach-san
```

### Bước 2: Cài SQL Server trên Windows

1. Cài SQL Server 2022/2025.
2. Tạo database `HotelManagementDB`.
3. Tạo tài khoản SQL riêng cho ứng dụng.

### Bước 3: Kiểm tra connection string

Mặc định trong `HotelManagement/appsettings.json`:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=HotelManagementDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=False"
```

Khi triển khai production, đặt lại `DefaultConnection` theo SQL Server thực tế của khách hàng
trong `appsettings.Production.json` hoặc biến môi trường hệ điều hành.

### Bước 4: Restore và chạy migration + seed

```bash
dotnet restore
dotnet run --project HotelManagement --urls http://localhost:5037
```

Ở lần chạy đầu, app sẽ:

- tự chạy migration (`context.Database.MigrateAsync()`),
- thực hiện seed role/user/dữ liệu mẫu.

### Bước 5: Truy cập ứng dụng

- Public: `http://localhost:5037/`
- Admin dashboard: `http://localhost:5037/admin`
- Login: `http://localhost:5037/login`

## 6. Cấu hình môi trường

Hệ thống dùng cấu hình `appsettings.json` + `appsettings.Production.json`.

### Cấu hình chính

| Key | Mô tả | Ví dụ |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Chuỗi kết nối SQL Server | `Server=.\\SQLEXPRESS;Database=HotelManagementDB;Trusted_Connection=True;...` |
| `HotelSettings:NoShowThresholdHours` | Ngưỡng auto-hủy no-show | `6` |
| `HotelSettings:CheckInTime` | Giờ check-in chuẩn | `14:00` |
| `HotelSettings:CheckOutTime` | Giờ check-out chuẩn | `12:00` |
| `WebsiteSettings:*` | Branding/Thông tin hiển thị website | logo, slogan, contact info |

Khuyến nghị: với production, ưu tiên dùng biến môi trường hệ thống để lưu thông tin nhạy cảm.

## 7. Tài khoản mặc định sau seed

`SeedData.cs` tạo sẵn:

- **Manager**
  - Email: `manager@hotel.com`
  - Password: `Hotel@123`
- **Receptionist**
  - Email: `le.reception@hotel.com`
  - Password: `Hotel@123`

Khuyến nghị: đổi mật khẩu ngay khi triển khai môi trường thật.

## 8. Cơ sở dữ liệu & migration

### Migration hiện có

Nằm trong:

- `HotelManagement/Infrastructure/Data/Migrations/`

### Lệnh EF Core

```bash
# Thêm migration
dotnet ef migrations add <MigrationName> --project HotelManagement --startup-project HotelManagement

# Update database
dotnet ef database update --project HotelManagement

# Rollback về migration cụ thể
dotnet ef database update <MigrationName> --project HotelManagement
```

## 9. Scripts & lệnh thường dùng

### Build / Run

```bash
# Build solution
dotnet build

# Run app
dotnet run --project HotelManagement --urls http://localhost:5037

```

### Test

```bash
# Chạy test (nếu có test project)
dotnet test
```

> Lưu ý: hiện solution chỉ có 1 project web (`HotelManagement.csproj`), chưa tách test project riêng.

## 10. Luồng nghiệp vụ chính

### 10.1 Booking lifecycle

1. Tạo booking (Admin hoặc yêu cầu từ Public)
2. Confirm booking
3. Check-in
4. Check-out + finalize invoice
5. Worker tự động xử lý no-show theo cấu hình giờ

### 10.2 Invoice lifecycle

1. Tạo invoice nháp từ booking
2. Thêm/xóa dịch vụ
3. Finalize (thanh toán)
4. Có thể split invoice khi đủ điều kiện

### 10.3 Dashboard

Dashboard tổng hợp:

- Doanh thu tháng
- Công suất phòng
- Khách trong tháng
- Phòng trống
- Booking gần đây

Đã cập nhật logic tăng/giảm theo dữ liệu thực tế giữa các kỳ.

## 11. Testing & quality checks

Checklist tối thiểu trước khi merge:

```bash
dotnet restore
dotnet build
dotnet test
```

Smoke test thủ công quan trọng:

1. Login Manager/Receptionist
2. CRUD Room/RoomType/Service/Guest
3. Tạo booking + check-in + checkout
4. Finalize invoice và xem trang invoice details
5. Kiểm tra dashboard load và biểu đồ

## 12. Deployment

### Khuyến nghị triển khai production

- Máy khách hàng dùng Windows + SQL Server cài trực tiếp.
- Chạy app ASP.NET Core trên IIS hoặc Windows Service.
- Thiết lập `ConnectionStrings:DefaultConnection` theo production.
- Bật HTTPS, giám sát logs, backup database định kỳ.

### Publish app

```bash
dotnet publish HotelManagement/HotelManagement.csproj -c Release -o ./publish
```

Sau đó chạy output publish bằng `dotnet HotelManagement.dll` hoặc cấu hình IIS để host ứng dụng.

## 13. Troubleshooting

### 1) Không kết nối được SQL Server

- Kiểm tra SQL Server service đang chạy trên Windows.
- Kiểm tra lại `Server`, `Database`, `User Id`/`Trusted_Connection` trong connection string.
- Kiểm tra quyền truy cập database của tài khoản ứng dụng.

### 2) Lỗi migration/model mismatch

```bash
dotnet ef database update --project HotelManagement --startup-project HotelManagement
```

Nếu vẫn lỗi, kiểm tra lại migration mới nhất trong `Infrastructure/Data/Migrations`.

### 3) Warning NU1900 khi build

Trong môi trường không truy cập được `nuget.org`, có thể thấy warning lấy vulnerability metadata. Đây là warning môi trường mạng, không nhất thiết là lỗi compile/runtime.

## 14. Ghi chú quan trọng

- Hệ thống đã **loại bỏ module Folio** để giảm độ phức tạp.
- Vẫn giữ nguyên các luồng vận hành chính: booking, invoice, checkout, dashboard.
- Toàn bộ định dạng locale đã chuẩn hóa cho Việt Nam:
  - tiền tệ: phân cách nghìn bằng `,`, không hiển thị `.00`
  - ngày tháng: `dd/MM/yyyy`

---

Tài liệu này là bản bàn giao production cho khách hàng.
