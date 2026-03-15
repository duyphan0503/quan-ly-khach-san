# HƯỚNG DẪN GIẢI TRÌNH & BÀN GIAO

**Minh Quang Luxury Hotel Management — Defense & Handover Guide**

> Tài liệu này giúp sinh viên trình bày đồ án tự tin, chạy demo ổn định,
> và bàn giao dự án đúng chuẩn kỹ thuật.

---

## Mục Lục

1. [Tổng Quan Đề Tài](#1-tổng-quan-đề-tài)
2. [Kiến Trúc Hệ Thống](#2-kiến-trúc-hệ-thống)
3. [Yêu Cầu Môi Trường](#3-yêu-cầu-môi-trường)
4. [Cấu Trúc Mã Nguồn Quan Trọng](#4-cấu-trúc-mã-nguồn-quan-trọng)
5. [Thiết Lập & Chạy Demo Trên Windows](#5-thiết-lập--chạy-demo-trên-windows)
6. [Khởi Tạo Dữ Liệu Mẫu Bằng SQL Script](#6-khởi-tạo-dữ-liệu-mẫu-bằng-sql-script)
7. [Kịch Bản Demo 7 Phút](#7-kịch-bản-demo-7-phút)
8. [Câu Hỏi Hội Đồng Thường Gặp](#8-câu-hỏi-hội-đồng-thường-gặp)
9. [Cách Trả Lời Khi Bị Hỏi Sâu](#9-cách-trả-lời-khi-bị-hỏi-sâu)
10. [Lỗi Thường Gặp & Cách Xử Lý Nhanh](#10-lỗi-thường-gặp--cách-xử-lý-nhanh)
11. [Checklist Trước Khi Bảo Vệ](#11-checklist-trước-khi-bảo-vệ)
12. [Checklist Bàn Giao Cho Người Nhận](#12-checklist-bàn-giao-cho-người-nhận)

---

## 1. Tổng Quan Đề Tài

### Mục tiêu

Xây dựng hệ thống quản lý khách sạn gồm:
- Khu vực **Public** cho khách xem phòng và gửi yêu cầu đặt phòng.
- Khu vực **Admin** cho nhân viên quản lý phòng, khách, booking, dịch vụ, hóa đơn và dashboard.

### Giá trị nghiệp vụ

- Quản lý xuyên suốt vòng đời: **booking -> check-in -> check-out -> invoice**.
- Chuẩn hóa dữ liệu khách sạn trên một hệ thống thống nhất.
- Hỗ trợ báo cáo nhanh bằng dashboard KPI.

---

## 2. Kiến Trúc Hệ Thống

### Kiến trúc tổng quát

```
Razor Pages (UI)
      -> Application Services (Business Logic)
      -> Repositories (Data Access)
      -> AppDbContext (EF Core)
      -> SQL Server
```

### Phân lớp N-Tier

| Tầng | Thư mục | Vai trò |
|---|---|---|
| Core | `HotelManagement/Core` | Entity, enum, constant nghiệp vụ |
| Infrastructure | `HotelManagement/Infrastructure` | DbContext, migration, repository, seed |
| Application | `HotelManagement/Application` | Service xử lý logic nghiệp vụ |
| UI | `HotelManagement/Areas` | Razor Pages cho Admin/Public/Identity |

### Điểm cần nhấn mạnh khi giải trình

- PageModel chỉ nhận input và gọi service.
- Service xử lý nghiệp vụ, validation, quy tắc domain.
- Repository tách biệt truy cập dữ liệu để dễ bảo trì.
- Có `BackgroundService` xử lý nghiệp vụ no-show.

---

## 3. Yêu Cầu Môi Trường

### Môi trường báo cáo/bàn giao

- Windows 10/11 hoặc Windows Server
- .NET SDK 10
- SQL Server 2022/2025 (Express/Developer đều được)
- `sqlcmd` hoặc SSMS

### Kiểm tra nhanh

```powershell
dotnet --version
sqlcmd -?
```

---

## 4. Cấu Trúc Mã Nguồn Quan Trọng

```
quan-ly-khach-san/
├── HotelManagement/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   ├── Application/Services/
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── SeedData.cs
│   │   │   ├── Migrations/
│   │   │   └── Scripts/seed_demo_data.sql
│   │   └── Repositories/
│   ├── Areas/Admin/
│   ├── Areas/Public/
│   └── Areas/Identity/
├── README.md
├── README_REPORT.md
└── BI_KIP_GIAI_TRINH.md
```

### File "phải thuộc" để trình bày

- `HotelManagement/Program.cs`
- `HotelManagement/Infrastructure/Data/AppDbContext.cs`
- `HotelManagement/Infrastructure/Data/SeedData.cs`
- `HotelManagement/Application/Services/BookingService.cs`
- `HotelManagement/Application/Services/InvoiceService.cs`
- `HotelManagement/Application/Services/DashboardService.cs`

---

## 5. Thiết Lập & Chạy Demo Trên Windows

### Bước 1: Clone source

```bash
git clone <REPO_URL>
cd quan-ly-khach-san
```

### Bước 2: Cấu hình SQL Server

- Tạo database `HotelManagementDB`.
- Cập nhật connection string trong `HotelManagement/appsettings.Production.json`.

Ví dụ:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=HotelManagementDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=False"
}
```

### Bước 3: Build và chạy

```bash
dotnet restore
dotnet build

dotnet run --project HotelManagement --urls http://localhost:5037
```

### Bước 4: Truy cập

- Public: `http://localhost:5037/`
- Admin: `http://localhost:5037/admin`
- Login: `http://localhost:5037/login`

---

## 6. Khởi Tạo Dữ Liệu Mẫu Bằng SQL Script

Dùng script:

- `HotelManagement/Infrastructure/Data/Scripts/seed_demo_data.sql`

Lệnh chạy:

```powershell
sqlcmd -S .\SQLEXPRESS -d HotelManagementDB -i HotelManagement\Infrastructure\Data\Scripts\seed_demo_data.sql
```

Mục tiêu script:
- Seed RoomTypes
- Seed Rooms
- Seed Services
- Seed Guests
- Idempotent: chạy lại vẫn an toàn

---

## 7. Kịch Bản Demo 7 Phút

### Mở đầu (30-45 giây)

"Đây là hệ thống quản lý khách sạn xây bằng ASP.NET Core Razor Pages, kiến trúc N-Tier,
CSDL SQL Server, hỗ trợ các nghiệp vụ chính từ đặt phòng đến xuất hóa đơn."

### Demo chính

1. Đăng nhập bằng tài khoản Manager.
2. Vào RoomType và Rooms để kiểm tra dữ liệu nền.
3. Tạo Guest mới hoặc chọn Guest có sẵn.
4. Tạo Booking cho khách.
5. Thực hiện check-in.
6. Thêm dịch vụ vào Invoice.
7. Check-out và finalize invoice.
8. Mở Dashboard và chỉ ra KPI vừa thay đổi.

### Kết thúc (20-30 giây)

"Hệ thống đáp ứng trọn luồng vận hành khách sạn: quản lý phòng, booking, dịch vụ,
hóa đơn và dashboard, đồng thời tổ chức code tách lớp để dễ bảo trì và mở rộng."

---

## 8. Câu Hỏi Hội Đồng Thường Gặp

### 1) Vì sao chọn Razor Pages?

Trả lời ngắn:
- Phù hợp hệ thống CRUD-heavy.
- Tốc độ triển khai nhanh cho đồ án.
- Dễ quản lý luồng server-side và xác thực.

### 2) Vì sao tách Service và Repository?

Trả lời ngắn:
- Tách business logic khỏi UI và data access.
- Dễ test hơn và dễ bảo trì hơn.
- Tuân thủ nguyên tắc kiến trúc nhiều lớp.

### 3) Vì sao dùng EF Core thay vì SQL thuần?

Trả lời ngắn:
- Quản lý migration rõ ràng.
- Truy vấn typed, giảm lỗi thủ công.
- Tích hợp tốt với SQL Server và .NET.

### 4) Hệ thống xử lý no-show thế nào?

Trả lời ngắn:
- Dùng `NoShowCancellationWorker` chạy nền.
- Dựa trên `HotelSettings:NoShowThresholdHours` để tự động cập nhật trạng thái booking.

### 5) Nếu mở rộng sản phẩm sau đồ án thì làm gì trước?

Trả lời ngắn:
- Bổ sung test tự động cho service quan trọng.
- Thêm audit log và phân quyền chi tiết hơn.
- Tối ưu dashboard bằng caching và query tuning.

---

## 9. Cách Trả Lời Khi Bị Hỏi Sâu

Khi không nhớ chi tiết code, dùng mẫu:

- "Em xin phép mở nhanh file service để trình bày đúng luồng xử lý."
- "Phần này em tách theo nguyên tắc N-Tier: UI gọi Service, Service gọi Repository."
- "Điểm chính em muốn nhấn mạnh là tách lớp để dễ bảo trì và tránh nhồi logic vào UI."

Nguyên tắc giữ bình tĩnh:
- Không đoán bừa.
- Kéo câu trả lời về kiến trúc và nghiệp vụ.
- Nếu cần, mở file minh họa trực tiếp.

---

## 10. Lỗi Thường Gặp & Cách Xử Lý Nhanh

### Lỗi 1: Không kết nối SQL Server

Kiểm tra:
- SQL Server service có đang chạy không.
- Connection string đúng instance/database chưa.
- Tài khoản có quyền vào DB chưa.

### Lỗi 2: Migrate chưa chạy

```bash
dotnet ef database update --project HotelManagement --startup-project HotelManagement
```

### Lỗi 3: Dashboard trống dữ liệu

Giải pháp:
- Chạy script seed SQL.
- Thực hiện thêm ít nhất 1 booking và 1 invoice finalize để có số liệu động.

### Lỗi 4: Build warning NU1900

Giải thích:
- Thường do máy không truy cập được `nuget.org`.
- Không phải lỗi logic của project nếu build vẫn thành công.

---

## 11. Checklist Trước Khi Bảo Vệ

- [ ] Build thành công (`dotnet build`).
- [ ] App chạy được trên cổng demo.
- [ ] SQL Server kết nối ổn định.
- [ ] Đã chạy seed SQL script.
- [ ] Đăng nhập được tài khoản demo.
- [ ] Chạy được kịch bản booking -> invoice -> dashboard.
- [ ] Chuẩn bị sẵn câu mở đầu + kết luận.

---

## 12. Checklist Bàn Giao Cho Người Nhận

### Mã nguồn & cấu hình

- [ ] Source code đầy đủ.
- [ ] `appsettings.Production.json` đã cấu hình đúng môi trường nhận bàn giao.
- [ ] Tài liệu `README.md`, `README_REPORT.md`, `BI_KIP_GIAI_TRINH.md` có đầy đủ.

### Database

- [ ] Database `HotelManagementDB` đã tạo.
- [ ] Migration đã chạy.
- [ ] Seed script đã chạy thành công.

### Vận hành

- [ ] Có tài khoản truy cập để kiểm thử nhanh.
- [ ] Có checklist xử lý sự cố cơ bản.
- [ ] Đã hướng dẫn người nhận luồng nghiệp vụ chính.

---

> Cập nhật lần cuối: 2026-03-15
>
> Phạm vi tài liệu: Hướng dẫn giải trình đồ án và bàn giao kỹ thuật ở mức thực hành.
