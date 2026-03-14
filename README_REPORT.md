# README_REPORT - Tài Liệu Tóm Tắt Để Báo Cáo

Tài liệu này dành cho sinh viên dùng khi demo và thuyết trình đồ án.
Nội dung tập trung vào: mục tiêu đề tài, kiến trúc, luồng nghiệp vụ, và checklist trình bày.

## 1. Mục tiêu đề tài

Xây dựng hệ thống quản lý khách sạn gồm 2 phần:
- Giao diện công khai (Public): xem phòng, gửi yêu cầu đặt phòng.
- Giao diện quản trị (Admin): quản lý phòng, loại phòng, khách, booking, dịch vụ, hóa đơn.

## 2. Công nghệ sử dụng

- Backend: ASP.NET Core Razor Pages (.NET 10)
- ORM: Entity Framework Core 10
- CSDL: SQL Server
- Auth: ASP.NET Core Identity (Manager, Receptionist)
- UI: Tailwind/DaisyUI/Flowbite

## 3. Kiến trúc hệ thống (N-Tier)

- `Core`: Entity, enum, hằng số nghiệp vụ.
- `Infrastructure`: DbContext, Repository, Migration, Seed.
- `Application`: Service xử lý nghiệp vụ.
- `Areas` (UI): PageModel và Razor Page.

Luồng gọi chuẩn:

`Razor Page -> PageModel -> Service -> Repository -> DbContext -> SQL Server`

## 4. Các luồng nghiệp vụ chính để demo

1. Quản lý phòng và loại phòng.
2. Tạo booking cho khách.
3. Check-in, check-out, phát sinh dịch vụ.
4. Finalize hóa đơn.
5. Dashboard xem KPI.

## 5. Dữ liệu mẫu để demo

Dữ liệu mẫu có thể seed bằng SQL script:

- File: `HotelManagement/Infrastructure/Data/Scripts/seed_demo_data.sql`

Ví dụ chạy trên Windows (SQL Server Express):

```powershell
sqlcmd -S .\SQLEXPRESS -d HotelManagementDB -i HotelManagement\Infrastructure\Data\Scripts\seed_demo_data.sql
```

## 6. Tài khoản mặc định

- Manager: `manager@hotel.com` / `Hotel@123`
- Receptionist: `le.reception@hotel.com` / `Hotel@123`

## 7. Checklist trước khi báo cáo

- Build chạy được: `dotnet build`
- App chạy được: `dotnet run --project HotelManagement --urls http://localhost:5037`
- DB có dữ liệu mẫu (đã migrate + seed)
- Demo được 1 kịch bản xuyên suốt: Tạo booking -> checkout -> invoice
- Dashboard hiển thị số liệu

## 8. Điểm nhấn khi thuyết trình

- Đã tách lớp rõ ràng theo N-Tier.
- Dữ liệu được quản lý qua service/repository, không viết logic trực tiếp ở UI.
- Có xử lý localization `vi-VN`.
- Có luồng background job cho nghiệp vụ no-show.

## 9. Lỗi thường gặp khi demo

- Sai connection string SQL Server.
- Chưa migrate DB.
- Cổng chạy app bị trùng.
- Dữ liệu seed chưa có nên dashboard trống.

## 10. Tài liệu liên quan

- `README.md`: tài liệu triển khai production.
- `BI_KIP_GIAI_TRINH.md`: hướng dẫn giải trình A-Z.
