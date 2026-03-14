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
- [5. Bắt đầu nhanh (local development)](#5-bắt-đầu-nhanh-local-development)
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
- **Database**: SQL Server 2025 (Docker chỉ dùng cho local dev trên Linux)
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
- SQL Server 2025
- Docker + Docker Compose (tùy chọn, chỉ dùng local dev trên Linux)
- SQL client tùy chọn (`sqlcmd`, Azure Data Studio, DBeaver)
- OS: Linux/macOS/Windows

## 5. Bắt đầu nhanh (local development)

### Bước 1: Clone source

```bash
git clone <REPO_URL>
cd quan-ly-khach-san
```

### Bước 2: Chuẩn bị SQL Server

Chọn 1 trong 2 cách:

- **Linux (dev nội bộ)**: chạy SQL Server bằng Docker local của bạn.
- **Windows (môi trường khách hàng/production)**: cài SQL Server trực tiếp trên máy chủ.

> Lưu ý: cấu hình Docker phục vụ dev nội bộ, không nằm trong mã nguồn push cho khách hàng.

### Bước 3: Kiểm tra connection string

Mặc định trong `HotelManagement/appsettings.json`:

```json
"DefaultConnection": "Server=localhost,1433;Database=HotelManagementDB;User Id=sa;Password=Hotel@123;TrustServerCertificate=True;MultipleActiveResultSets=False"
```

Nếu bạn dùng password/port khác, cập nhật lại tương ứng.

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

Hiện tại dự án dùng `appsettings.json` + `appsettings.Development.json` (không có `.env` mặc định).

### Cấu hình chính

| Key | Mô tả | Ví dụ |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Chuỗi kết nối SQL Server | `Server=localhost,1433;Database=HotelManagementDB;User Id=sa;Password=...` |
| `HotelSettings:NoShowThresholdHours` | Ngưỡng auto-hủy no-show | `6` |
| `HotelSettings:CheckInTime` | Giờ check-in chuẩn | `14:00` |
| `HotelSettings:CheckOutTime` | Giờ check-out chuẩn | `12:00` |
| `WebsiteSettings:*` | Branding/Thông tin hiển thị website | logo, slogan, contact info |

### User Secrets

Project có `UserSecretsId` trong `.csproj`, có thể dùng để tách secret local:

```bash
dotnet user-secrets --project HotelManagement set "ConnectionStrings:DefaultConnection" "<YOUR_CONNECTION_STRING>"
```

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

# Hot reload
dotnet watch --project HotelManagement run --urls http://localhost:5037
```

### Script có sẵn

```bash
# Script tiện ích chạy dev (kill port 5037 + dotnet watch)
./run-dev.sh
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

Docker chỉ được dùng cho môi trường dev Linux để tiện chạy SQL Server local.
Mã cấu hình Docker dev nội bộ không phát hành lên GitHub dành cho khách hàng.

### Khuyến nghị triển khai production

- Máy khách hàng dùng Windows + SQL Server cài trực tiếp (không phụ thuộc Docker).
- Chạy app ASP.NET Core riêng (IIS/Kestrel service) và trỏ tới SQL Server production.
- Thiết lập biến môi trường production cho `ConnectionStrings`.
- Bật HTTPS, giám sát logs, backup database định kỳ.

### Publish app

```bash
dotnet publish HotelManagement/HotelManagement.csproj -c Release -o ./publish
```

Sau đó chạy output publish bằng `dotnet HotelManagement.dll` hoặc đóng gói container tùy hạ tầng.

## 13. Troubleshooting

### 1) Không kết nối được SQL Server

- Nếu bạn đang dev trên Linux bằng Docker, kiểm tra container SQL:

```bash
docker ps
```

- Kiểm tra port 1433 có mở:

```bash
ss -lntp | rg 1433
```

- Đảm bảo `MSSQL_SA_PASSWORD` khớp với `appsettings.json`.

### 2) Lỗi migration/model mismatch

```bash
dotnet ef database update --project HotelManagement --startup-project HotelManagement
```

Nếu vẫn lỗi, kiểm tra lại migration mới nhất trong `Infrastructure/Data/Migrations`.

### 3) Cổng 5037 bị chiếm

```bash
./run-dev.sh
```

Script sẽ tự giải phóng cổng 5037 và chạy lại app.

### 4) Warning NU1900 khi build

Trong môi trường không truy cập được `nuget.org`, có thể thấy warning lấy vulnerability metadata. Đây là warning môi trường mạng, không nhất thiết là lỗi compile/runtime.

## 14. Ghi chú quan trọng

- Hệ thống đã **loại bỏ module Folio** để giảm độ phức tạp.
- Vẫn giữ nguyên các luồng vận hành chính: booking, invoice, checkout, dashboard.
- Toàn bộ định dạng locale đã chuẩn hóa cho Việt Nam:
  - tiền tệ: phân cách nghìn bằng `,`, không hiển thị `.00`
  - ngày tháng: `dd/MM/yyyy`

---

Nếu bạn muốn, mình có thể tách thêm:

1. `README_DEV.md` (nội bộ dev)
2. `README_DEPLOY.md` (playbook triển khai khách hàng)
