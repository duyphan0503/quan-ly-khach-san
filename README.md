<div align="center">
  <h1 align="center">🏨 Minh Quang Luxury Hotel Management</h1>
  <p align="center">
    <strong>Hệ thống Quản lý Khách sạn Chuyên nghiệp xây dựng trên nền tảng ASP.NET Core 10 Razor Pages và Kiến trúc N-Tier.</strong>
  </p>
  <p align="center">
    <img src="https://img.shields.io/badge/.NET-10.0-blue.svg?logo=dotnet" alt=".NET 10" />
    <img src="https://img.shields.io/badge/EF%20Core-10.0-68217A.svg?logo=nuget" alt="EF Core 10" />
    <img src="https://img.shields.io/badge/SQL_Server-2022-CC292B.svg?logo=microsoft-sql-server" alt="SQL Server" />
    <img src="https://img.shields.io/badge/Architecture-N--Tier-brightgreen.svg" alt="N-Tier" />
  </p>
</div>

---

## 📖 Giới thiệu (Overview)

**Minh Quang Luxury** là ứng dụng web toàn diện được thiết kế để xử lý trọn vẹn vòng đời nghiệp vụ vận hành khách sạn. Hệ thống phân chia rõ ràng hai khu vực:

- **Public Portal:** Dành cho khách hàng xem danh sách phòng, tham khảo dịch vụ và gửi yêu cầu đặt phòng (Booking Request).
- **Admin Portal (Back-office):** Dành cho Lễ tân và Quản lý để vận hành mọi khía cạnh như nhận/trả phòng, dịch vụ phát sinh, xuất hóa đơn và theo dõi trực quan trạng thái khách sạn qua Dashboard.

Đặc biệt, dự án được xây dựng theo chuẩn **Kiến trúc N-Tier (Monolith)**, giúp tách biệt logic nghiệp vụ khỏi giao diện, tối ưu khả năng bảo trì và dễ dàng mở rộng trong tương lai.

## ✨ Tính năng nổi bật (Key Features)

### 📊 Phân hệ Quản trị (Admin)

- **Dashboard & Báo cáo:** Tóm tắt KPI (Doanh thu, Công suất phòng, Lịch sử Booking) qua biểu đồ trực quan mạnh mẽ (sử dụng _ChartJSCore_).
- **Quản lý Danh mục (Catalog):** Quản trị linh hoạt các loại phòng (Room Type), danh sách phòng (Room), cấu hình dịch vụ (Service) và hồ sơ khách hàng (Guest).
- **Vòng đời Đặt phòng:** Quản lý trọn luồng quy chuẩn: `Booking (Đặt trước) ➔ Check-in (Nhận phòng) ➔ Room Service (Phát sinh dịch vụ) ➔ Check-out & Invoice (Trả phòng và xuất hóa đơn)`.
- **Identity & Phân quyền:** Quản lý tài khoản, định danh và phân quyền chặt chẽ cho vai trò `Manager` (Quản lý) và `Receptionist` (Lễ tân) thông qua thư viện _ASP.NET Core Identity_.

### 🌐 Phân hệ Khách hàng (Public)

- **Danh mục phòng đa phương tiện:** Tra cứu chi tiết hạng phòng, lọc theo tiêu chí không gian, hình ảnh minh họa chân thực xử lý qua _SixLabors.ImageSharp_.
- **Yêu cầu Booking:** Đặt phòng trực tuyến thông qua hệ thống giao diện tối giản và thân thiện.

### ⚙️ Tính năng nền (Background Jobs)

- **Bắt lỗi No-show tự động:** Tích hợp worker service `NoShowCancellationWorker` chạy ngầm để tự động quét và hủy các booking khách không đến `check-in` sau một thời gian cấu hình tùy chỉnh (`HotelSettings:NoShowThresholdHours`).

## 🛠 Tech Stack (Công nghệ sử dụng)

- **Framework:** .NET 10 (C# 14), ASP.NET Core Razor Pages
- **Database / ORM:** SQL Server hiện đại, Entity Framework Core 10
- **Security:** ASP.NET Core Identity (Authentication, Authorization & Password Hashing)
- **UI / UX:** Flowbite / Tailwind CSS / DaisyUI (Giao diện đáp ứng)
- **Thư viện bên thứ ba:** ChartJSCore (Biểu diễn thống kê), SixLabors.ImageSharp (Xử lý tập tin đồ họa)

## 🏗 Kiến trúc dự án (Architecture)

Kiến trúc Monolith tuân thủ nghiêm ngặt mô hình N-Tier, phân cấp rõ ràng trách nhiệm của từng layer.

**Luồng dữ liệu cơ bản:**
`UI (Razor Pages/PageModel) ➔ Application (Business Service) ➔ Infrastructure (Repository) ➔ EF DbContext ➔ Database`

```text
quan-ly-khach-san/
├── HotelManagement/
│   ├── Core/           (Models, Entities, Enums, Constants)
│   ├── Infrastructure/ (DbContext, Repositories, Identity Integration, Data Seeders)
│   ├── Application/    (Business Logic Services, Interfaces)
│   ├── Areas/          (Razor Pages UI cho Admin, Public, Identity)
│   ├── Program.cs      (Dependency Injection, Middleware pipeline, Configs)
│   └── appsettings.json
├── BI_KIP_GIAI_TRINH.md (Bí kíp bảo vệ đồ án / Bàn giao)
└── README.md            (Tài liệu tổng quan này)
```

## 🚀 Hướng dẫn Cài đặt & Khởi chạy (Local Setup)

### Yêu cầu môi trường

- .NET SDK 10.0+
- SQL Server (Express / Developer / Standard)
- Git (Để clone mã nguồn)

### Các bước triển khai

**1. Clone dự án**

```bash
git clone <đường_dẫn_repo_của_bạn>
cd quan-ly-khach-san
```

**2. Khôi phục packages & tools**

```bash
dotnet tool restore
dotnet restore
```

**3. Thiết lập Database (Connection String)**
Mở file `HotelManagement/appsettings.json` và cập nhật chuỗi kết nối `ConnectionStrings:DefaultConnection` trỏ tới Server SQL Server bạn đang dùng.

> _Mặc định:_ `"Server=.;Database=HotelManagementDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=False"`

**4. Áp dụng Database Migrations**
Hệ thống sử dụng EF Core công cụ Migration để tự động hóa sinh database schema. Chạy lệnh:

```bash
dotnet ef database update --project HotelManagement --startup-project HotelManagement
```

**5. Khởi chạy Ứng dụng**

```bash
dotnet run --project HotelManagement --urls "http://localhost:5037"
```

> **Lưu ý Seed Data:** Ứng dụng sẽ tự động kích hoạt tiến trình Seed (`SeedData.Initialize(...)`) để đổ dữ liệu mẫu sẵn có (Phòng, dịch vụ, tài khoản demo) mượt mà ngay lần chạy đầu tiên.

## 🔑 Tài khoản Demo (Default Accounts)

Mật khẩu chung và bảo mật mặc định cho các tài khoản bên dưới là: **`Hotel@123`**

- **Tài khoản Quản lý (Manager):** `manager@hotel.com`
- **Tài khoản Lễ tân (Receptionist):** `le.reception@hotel.com`

## _(Lưu ý: Mật khẩu này được seed tự động và nên được thay đổi hoặc vô hiệu hóa khi di chuyển lên môi trường Production thực tế)._

<div align="center">
  <i>Được xây dựng đảm bảo các chuẩn mực Coding, trải nghiệm ổn định và nền tảng mở rộng vững chắc!</i>
</div>
