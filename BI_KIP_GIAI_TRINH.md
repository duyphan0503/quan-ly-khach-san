# HƯỚNG DẪN GIẢI TRÌNH & BÀN GIAO (BẢN CHI TIẾT CHO SINH VIÊN MỚI)

**Minh Quang Luxury Hotel Management — Defense & Handover Guide**

Mục tiêu của tài liệu này là giúp bạn **nắm bản chất hệ thống từ đầu**, kể cả khi bạn còn yếu phần code:

- hiểu ứng dụng chạy từ đâu đến đâu,
- biết mỗi thư mục chịu trách nhiệm gì,
- demo đúng luồng nghiệp vụ mà không bị rối,
- trả lời câu hỏi hội đồng dựa trên file thật trong source.

---

## 1) TÓM TẮT ĐỀ TÀI BẰNG NGÔN NGỮ DỄ HIỂU

Đây là hệ thống quản lý khách sạn có 2 mặt:

1. **Public (khách vãng lai):** xem phòng, đọc thông tin, gửi yêu cầu đặt phòng.
2. **Admin (nhân viên/quản lý):** quản lý phòng, khách, booking, dịch vụ, hóa đơn, dashboard.

### Luồng nghiệp vụ cốt lõi cần thuộc

`Đặt phòng -> nhận phòng (check-in) -> dùng dịch vụ -> trả phòng (check-out) -> thanh toán hóa đơn`

Nếu bạn chỉ nhớ 1 thứ để đi bảo vệ, hãy nhớ luồng trên.

---

## 2) CÁCH HỆ THỐNG CHẠY (TƯ DUY TỪ GỐC)

Hãy tưởng tượng 1 request đi theo tuyến sau:

```text
Người dùng bấm nút trên trang Razor
-> PageModel trong Areas xử lý input
-> Gọi Application Service (nơi giữ luật nghiệp vụ)
-> Service gọi Repository (nơi truy cập dữ liệu)
-> Repository dùng AppDbContext (EF Core)
-> SQL Server
-> Trả kết quả ngược về UI
```

### Tại sao phải tách nhiều lớp như vậy?

- **UI**: chỉ nhận dữ liệu từ form và hiển thị.
- **Service**: quyết định đúng/sai theo nghiệp vụ khách sạn.
- **Repository**: chuyên query DB.
- **DbContext**: map bảng, quan hệ, ràng buộc.

Nhờ tách lớp, code dễ đọc, dễ sửa, tránh việc “trộn form + SQL + logic” vào một chỗ.

---

## 3) BẢN ĐỒ THƯ MỤC (HỌC THEO KIỂU “XEM LÀ BIẾT VIỆC”)

## 3.1 Gốc dự án

- `quan-ly-khach-san.sln`: solution tổng.
- `README.md`: mô tả nhanh.
- `BI_KIP_GIAI_TRINH.md`: tài liệu bạn đang đọc.

## 3.2 `HotelManagement/` (project chính)

### A. `Core/`

Nơi chứa “định nghĩa nghiệp vụ thuần”:

- `Models/`: các thực thể như `Booking`, `Room`, `Guest`, `Invoice`...
- `Models/Enums/`: các enum trạng thái như `BookingStatus`, `RoomStatus`, `InvoiceStatus`.
- `Constants/`: hằng số thương hiệu/hệ thống.

**Hiểu đơn giản:** Core = từ điển nghiệp vụ của hệ thống.

### B. `Infrastructure/`

Nơi làm việc với hạ tầng kỹ thuật:

- `Data/AppDbContext.cs`: cấu hình EF Core, quan hệ giữa bảng, index, check constraint.
- `Data/SeedData.cs`: tạo role/user và dữ liệu nền theo cách idempotent.
- `Repositories/`: code truy vấn DB theo từng domain.
- `Identity/`: tùy biến thông báo lỗi Identity tiếng Việt.

**Hiểu đơn giản:** Infrastructure = phần “nối hệ thống với database/identity”.

### C. `Application/`

Nơi đặt **business logic**:

- `Services/BookingService.cs`: luật đặt phòng, kiểm tra trùng lịch, đổi trạng thái.
- `Services/InvoiceService.cs`: tạo hóa đơn nháp, thêm dịch vụ, finalize thanh toán, tách hóa đơn.
- `Services/DashboardService.cs`: tổng hợp KPI dashboard.
- `Services/NoShowCancellationWorker.cs`: job nền hủy no-show định kỳ.
- `ViewModels/`: model dữ liệu phục vụ màn hình dashboard.

**Hiểu đơn giản:** Application = “bộ não nghiệp vụ”.

### D. `Areas/`

Nơi chứa giao diện Razor Pages:

- `Areas/Admin/Pages`: trang nghiệp vụ nội bộ.
- `Areas/Public/Pages`: trang công khai cho khách.
- `Areas/Identity/Pages`: trang đăng nhập, quản lý tài khoản.

**Hiểu đơn giản:** Areas = nơi người dùng nhìn thấy và thao tác.

### E. `wwwroot/`

Tài nguyên tĩnh: CSS, JS, ảnh, thư viện frontend.

---

## 4) STARTUP FLOW THỰC TẾ (ĐỌC THEO `Program.cs`)

Khi chạy app, hệ thống đi qua các bước chính:

1. Đọc cấu hình từ `appsettings.json` và môi trường.
2. Đăng ký DI cho:
   - DbContext,
   - Repository,
   - Service,
   - Hosted worker (`NoShowCancellationWorker`).
3. Cấu hình Identity + role.
4. Cấu hình localization theo `vi-VN`.
5. Cấu hình middleware:
   - exception handling,
   - status code handling (`/notfound`, `/error`),
   - static files,
   - routing,
   - auth.
6. Khi app khởi động xong:
   - chạy migration (`MigrateAsync`),
   - gọi seed (`SeedData.Initialize`).

### Ý nghĩa thực tế

- Bạn không phải seed thủ công bằng script SQL trong quy trình thông thường.
- DB schema và dữ liệu nền được tự đồng bộ theo startup flow.

---

## 5) DATABASE FLOW THỰC TẾ (ĐỌC THEO `AppDbContext.cs`)

`AppDbContext.cs` là nơi “chốt luật dữ liệu”. Các điểm quan trọng:

- khai báo DbSet cho các bảng chính (`Bookings`, `Rooms`, `Guests`, `Invoices`, ...),
- cấu hình quan hệ giữa các bảng,
- cấu hình delete behavior,
- cấu hình index để tối ưu truy vấn,
- có check constraint kiểm tra logic ngày đặt phòng (ví dụ check-out phải sau check-in).

### Câu bạn có thể dùng khi bảo vệ

“Ngoài validate ở Service, nhóm còn đặt ràng buộc dữ liệu ở tầng DbContext/database để tăng độ an toàn dữ liệu.”

---

## 6) SEED FLOW THỰC TẾ (ĐỌC THEO `SeedData.cs`)

Seed data chạy theo tư duy **idempotent**:

- chạy nhiều lần không tạo trùng không cần thiết,
- đảm bảo có role chuẩn,
- đảm bảo có user demo,
- đảm bảo có dữ liệu nền để demo nhanh (room type, room, service, guest).

### Tài khoản demo mặc định

- `manager@hotel.com` / `Hotel@123`
- `le.reception@hotel.com` / `Hotel@123`

> Lưu ý: khi bàn giao môi trường thật, phải đổi mật khẩu mặc định ngay.

---

## 7) LUỒNG NGHIỆP VỤ CHÍNH (ĐỌC THEO SERVICE)

## 7.1 Booking (`BookingService.cs`)

Các rule quan trọng:

1. `CheckOut` phải lớn hơn `CheckIn`.
2. Phòng không được trùng lịch trong cùng khoảng thời gian.
3. Tổng tiền phòng = đơn giá phòng \* số đêm.
4. Khi đổi trạng thái booking, trạng thái phòng cập nhật theo:
   - Confirmed -> Reserved
   - CheckedIn -> Occupied
   - CheckedOut/Cancelled -> Available

### Điểm mạnh kỹ thuật

- Dùng transaction cho các thao tác nhiều bảng (booking + room).
- Có xử lý đặt nhiều phòng cùng lúc (`CreateMultipleAsync`) và group code.
- Có auto-cancel no-show dựa trên cấu hình thời gian.

## 7.2 Invoice (`InvoiceService.cs`)

Các luồng quan trọng:

1. Tạo hóa đơn nháp theo booking (nếu đã có thì tái sử dụng).
2. Dòng mặc định trong hóa đơn: tiền phòng theo số đêm.
3. Thêm/xóa dịch vụ trên hóa đơn pending.
4. Finalize thanh toán:
   - cập nhật invoice -> Paid,
   - cập nhật booking -> CheckedOut,
   - cập nhật room -> Available.
5. Có checkout cả nhóm phòng và phân bổ tax/discount theo tỷ lệ subtotal.
6. Có tách hóa đơn (`SplitInvoiceAsync`) để chia chi tiết thanh toán.

### Điểm mạnh kỹ thuật

- Luôn dùng transaction cho nghiệp vụ tài chính đa bước.
- Tính toán subtotal/tax/discount/grand total rõ ràng.

## 7.3 Dashboard (`DashboardService.cs`)

Dashboard không hard-code số liệu, mà tổng hợp từ repository:

- doanh thu tổng,
- doanh thu tháng,
- thay đổi so với tháng trước,
- công suất phòng,
- số khách,
- booking gần đây,
- phân bổ theo loại phòng.

### Công thức cần nhớ

`OccupancyRate = OccupiedRoomNights / RoomCapacityTheoTháng * 100`

## 7.4 Worker no-show (`NoShowCancellationWorker.cs`)

Worker chạy nền theo chu kỳ ~5 phút:

1. Tạo scope DI,
2. gọi `AutoCancelNoShowAsync(now)`,
3. log kết quả thành công/thất bại.

Ý nghĩa: giảm booking treo, tự giải phóng phòng nếu khách không đến đúng hạn no-show.

---

## 8) HƯỚNG DẪN CHẠY TRÊN WINDOWS (THỰC CHIẾN)

## 8.1 Điều kiện môi trường

- Windows 10/11,
- .NET SDK 10+,
- SQL Server.

Kiểm tra:

```powershell
dotnet --version
```

## 8.2 Build và chạy

### Cấu hình SQL Server local (SQL Authentication)

Nếu máy bạn dùng tài khoản SQL Server mặc định `sa`, thông tin hiện tại là:

- User: `sa`
- Password: `000000`

Chuỗi kết nối mẫu trong `HotelManagement/appsettings.json`:

```json
"ConnectionStrings": {
   "DefaultConnection": "Server=.;Database=HotelManagementDB;User Id=sa;Password=000000;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=False"
}
```

> Nếu bạn vẫn dùng `Trusted_Connection=True` (Windows Authentication) thì không cần `sa`/password.

```bash
dotnet tool restore
dotnet restore
dotnet build
dotnet run --project HotelManagement
```

Local mặc định: `http://localhost:5037`

## 8.3 URL thường dùng

- Public: `http://localhost:5037/`
- Admin: `http://localhost:5037/admin`
- Login alias: `http://localhost:5037/login`

---

## 9) KỊCH BẢN DEMO 7-10 PHÚT (DỄ ĂN ĐIỂM)

## 9.1 Mở đầu (40 giây)

“Đề tài em xây theo Razor Pages + N-Tier, tách rõ UI/Service/Repository.
Hệ thống xử lý trọn luồng đặt phòng đến thanh toán, có dashboard KPI và worker no-show chạy nền.”

## 9.2 Demo nghiệp vụ (6-8 phút)

1. Đăng nhập Manager.
2. Vào RoomType/Room để chứng minh có dữ liệu nền.
3. Tạo hoặc chọn Guest.
4. Tạo Booking (show validate ngày + kiểm tra phòng trùng lịch).
5. Chuyển CheckedIn.
6. Vào Invoice thêm dịch vụ.
7. Finalize checkout.
8. Mở Dashboard, cho thấy KPI thay đổi theo giao dịch vừa tạo.

## 9.3 Chốt (20-30 giây)

“Điểm chính của hệ thống là tính nhất quán dữ liệu qua transaction,
tách lớp rõ ràng để dễ bảo trì, và có thể mở rộng báo cáo/phân quyền nâng cao sau đồ án.”

---

## 10) “HỌC NHANH TRONG 3 NGÀY” CHO NGƯỜI YẾU CODE

## Ngày 1: Hiểu luồng tổng

Đọc theo thứ tự:

1. `Program.cs`
2. `AppDbContext.cs`
3. `SeedData.cs`

Mục tiêu: biết app khởi động thế nào, DB dựng ra sao, dữ liệu demo vào từ đâu.

## Ngày 2: Hiểu nghiệp vụ chính

Đọc:

1. `BookingService.cs`
2. `InvoiceService.cs`

Mục tiêu: thuộc rule nghiệp vụ để trả lời hội đồng.

## Ngày 3: Hiểu màn hình và luồng request

Đọc:

1. `Areas/Admin/Pages/Bookings/*`
2. `Areas/Admin/Pages/Invoices/*`
3. `DashboardService.cs` + trang dashboard

Mục tiêu: biết người dùng bấm ở đâu thì service nào chạy.

---

## 11) CÂU HỎI HỘI ĐỒNG HAY HỎI + CÁCH TRẢ LỜI GỌN

### Q1. Vì sao không viết SQL trực tiếp hết?

Gợi ý trả lời:

“Nhóm dùng EF Core để quản lý migration và model mapping đồng bộ. Khi cần tối ưu truy vấn vẫn có thể tinh chỉnh ở repository.”

### Q2. Vì sao cần Service layer?

“Service gom toàn bộ luật nghiệp vụ. UI chỉ nhận input và hiển thị, không xử lý business rule.”

### Q3. Làm sao đảm bảo không lỗi dữ liệu khi checkout?

“Nghiệp vụ checkout chạy transaction: cập nhật invoice, booking, room trong cùng đơn vị công việc.”

### Q4. No-show xử lý thế nào?

“Worker nền chạy định kỳ, gọi service auto-cancel theo deadline cấu hình trong HotelSettings.”

### Q5. Nếu mở rộng hệ thống sau đồ án?

“Ưu tiên thêm automated test, audit log, phân quyền chi tiết và báo cáo vận hành nâng cao.”

---

## 12) DEBUG CHECKLIST (KHI DEMO BỊ LỖI)

## 12.1 Không vào được DB

- kiểm tra SQL Server service chạy,
- kiểm tra connection string trong `appsettings.json`,
- kiểm tra quyền account Windows/SQL.

## 12.2 Nghi ngờ migration chưa lên

```bash
dotnet ef migrations list --project HotelManagement --startup-project HotelManagement
dotnet ef database update --project HotelManagement --startup-project HotelManagement
```

## 12.3 Seed không có dữ liệu

- đặt breakpoint trong `SeedData.Initialize(...)`,
- kiểm tra startup có gọi migration + seed không,
- kiểm tra DB đang trỏ đúng instance không.

## 12.4 Dashboard chưa đổi số

- kiểm tra invoice đã `Paid` chưa,
- kiểm tra booking đã `CheckedOut` chưa,
- tạo thêm giao dịch mới để thấy biến động theo tháng.

---

## 13) BREAKPOINT NÊN ĐẶT KHI GIẢI THÍCH LIVE

Nếu hội đồng hỏi sâu, bạn mở debug theo thứ tự này:

1. `Program.cs` tại đoạn migrate + seed startup.
2. `BookingService.CreateAsync` (validate + transaction room status).
3. `BookingService.UpdateStatusAsync` (mapping status booking -> room).
4. `InvoiceService.GetOrCreateDraftInvoiceAsync`.
5. `InvoiceService.FinalizeInvoicesAsync` (checkout + payment transaction).
6. `DashboardService.GetDashboardDataAsync` (KPI tổng hợp).

Với cách này, bạn vừa trả lời được kiến trúc, vừa trả lời được nghiệp vụ bằng bằng chứng runtime.

---

## 14) CHECKLIST TRƯỚC BUỔI BẢO VỆ

- [ ] `dotnet build` thành công.
- [ ] app chạy được local ổn định.
- [ ] đăng nhập được tài khoản demo.
- [ ] demo trơn tru luồng booking -> check-in -> invoice -> checkout -> dashboard.
- [ ] thuộc tối thiểu 5 file trọng tâm: `Program.cs`, `AppDbContext.cs`, `SeedData.cs`, `BookingService.cs`, `InvoiceService.cs`.

---

## 15) CHECKLIST BÀN GIAO CHO NGƯỜI NHẬN

## 15.1 Mã nguồn

- [ ] source đầy đủ + hướng dẫn chạy.
- [ ] cấu hình môi trường production đã tách khỏi local.
- [ ] tài liệu README + tài liệu giải trình đồng bộ phiên bản.

## 15.2 Database

- [ ] xác nhận migration chạy được trên môi trường nhận bàn giao.
- [ ] xác nhận seed data tạo được role/user/dữ liệu nền.

## 15.3 Vận hành

- [ ] có tài khoản UAT cho người nhận kiểm thử nhanh.
- [ ] có checklist xử lý sự cố cơ bản.
- [ ] người nhận hiểu rõ luồng nghiệp vụ cốt lõi và vị trí file liên quan.

---

## 16) CÂU CHỐT KHI BẠN CĂNG THẲNG LÚC BẢO VỆ

Khi bị hỏi khó, bạn không cần trả lời dài. Chỉ cần theo mẫu:

“Phần này em xin mở file service tương ứng để giải thích đúng theo code đang chạy, tránh trả lời cảm tính.”

Câu này giúp bạn:

- giữ bình tĩnh,
- trả lời dựa trên sự thật kỹ thuật,
- tạo cảm giác làm việc chuyên nghiệp.

---

> Cập nhật lần cuối: 2026-03-15
