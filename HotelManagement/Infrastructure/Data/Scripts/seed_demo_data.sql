/*
  Script seed dữ liệu demo cho báo cáo môn học.
  Mục tiêu: dễ chạy, idempotent (chạy lại không bị duplicate dữ liệu chính).
  Áp dụng cho SQL Server.
*/

SET NOCOUNT ON;

/* =========================
   1) SEED ROOM TYPES
   ========================= */
IF NOT EXISTS (SELECT 1 FROM RoomTypes WHERE Name = N'Standard')
BEGIN
    INSERT INTO RoomTypes (Name, BasePrice, MaxOccupancy, ImageUrl, Description, Amenities)
    VALUES (N'Standard', 500000, 2, '/images/room_standard.jpg',
            N'Phòng tiêu chuẩn thoải mái, phù hợp cho khách du lịch cá nhân hoặc cặp đôi.',
            N'WiFi, TV, Điều hòa, Nóng lạnh');
END

IF NOT EXISTS (SELECT 1 FROM RoomTypes WHERE Name = N'Deluxe')
BEGIN
    INSERT INTO RoomTypes (Name, BasePrice, MaxOccupancy, ImageUrl, Description, Amenities)
    VALUES (N'Deluxe', 900000, 2, '/images/room_deluxe.jpg',
            N'Phòng cao cấp với view hồ bơi và ban công thoáng đãng.',
            N'WiFi, Smart TV, Điều hòa, Minibar, Tủ lạnh');
END

IF NOT EXISTS (SELECT 1 FROM RoomTypes WHERE Name = N'Suite')
BEGIN
    INSERT INTO RoomTypes (Name, BasePrice, MaxOccupancy, ImageUrl, Description, Amenities)
    VALUES (N'Suite', 1500000, 4, '/images/room_suite.jpg',
            N'Phòng Suite rộng rãi với phòng khách riêng biệt, lý tưởng cho gia đình.',
            N'WiFi, Smart TV, Điều hòa, Minibar, Bồn tắm, Phòng khách riêng');
END

IF NOT EXISTS (SELECT 1 FROM RoomTypes WHERE Name = N'VIP')
BEGIN
    INSERT INTO RoomTypes (Name, BasePrice, MaxOccupancy, ImageUrl, Description, Amenities)
    VALUES (N'VIP', 3000000, 4, '/images/room_vip.jpg',
            N'Trải nghiệm đẳng cấp bậc nhất với dịch vụ quản gia riêng và view toàn cảnh thành phố.',
            N'WiFi, Smart TV 4K, Điều hòa, Minibar cao cấp, Bồn tắm Jacuzzi, Ban công, Butler');
END

/* =========================
   2) SEED ROOMS
   ========================= */
DECLARE @StandardId INT = (SELECT TOP 1 Id FROM RoomTypes WHERE Name = N'Standard');
DECLARE @DeluxeId INT = (SELECT TOP 1 Id FROM RoomTypes WHERE Name = N'Deluxe');
DECLARE @SuiteId INT = (SELECT TOP 1 Id FROM RoomTypes WHERE Name = N'Suite');
DECLARE @VipId INT = (SELECT TOP 1 Id FROM RoomTypes WHERE Name = N'VIP');

IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '101') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('101', @StandardId, 1);
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '102') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('102', @StandardId, 1);
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '103') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('103', @StandardId, 1);
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '104') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('104', @DeluxeId, 1);
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '201') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('201', @DeluxeId, 2);
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '202') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('202', @DeluxeId, 2);
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '203') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('203', @SuiteId, 2);
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '301') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('301', @SuiteId, 3);
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '302') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('302', @VipId, 3);
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '401') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('401', @VipId, 4);
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = '402') INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor) VALUES ('402', @VipId, 4);

/* =========================
   3) SEED SERVICES
   ========================= */
IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = N'Giặt ủi')
    INSERT INTO Services (Name, Price, Unit) VALUES (N'Giặt ủi', 50000, N'lần');

IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = N'Minibar')
    INSERT INTO Services (Name, Price, Unit) VALUES (N'Minibar', 100000, N'lần');

IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = N'Spa & Massage')
    INSERT INTO Services (Name, Price, Unit) VALUES (N'Spa & Massage', 300000, N'giờ');

IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = N'Ăn sáng Buffet')
    INSERT INTO Services (Name, Price, Unit) VALUES (N'Ăn sáng Buffet', 80000, N'phần');

IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = N'Xe đưa đón SB')
    INSERT INTO Services (Name, Price, Unit) VALUES (N'Xe đưa đón SB', 250000, N'lượt');

IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = N'Phòng họp')
    INSERT INTO Services (Name, Price, Unit) VALUES (N'Phòng họp', 500000, N'giờ');

IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = N'Dịch vụ phòng')
    INSERT INTO Services (Name, Price, Unit) VALUES (N'Dịch vụ phòng', 30000, N'lần');

/* =========================
   4) SEED GUESTS
   ========================= */
IF NOT EXISTS (SELECT 1 FROM Guests WHERE CCCD = N'079200001234')
BEGIN
    INSERT INTO Guests (FullName, CCCD, Phone, Email, Address, Nationality, AvatarUrl)
    VALUES (N'Nguyễn Văn An', N'079200001234', N'0901234567', N'an.nguyen@gmail.com',
            N'123 Lê Văn Sỹ, Q.3, TP.HCM', N'Việt Nam', '/uploads/avatars/an.jpg');
END

IF NOT EXISTS (SELECT 1 FROM Guests WHERE CCCD = N'001200005678')
BEGIN
    INSERT INTO Guests (FullName, CCCD, Phone, Email, Address, Nationality, AvatarUrl)
    VALUES (N'Trần Thị Bích', N'001200005678', N'0912345678', N'bich.tran@outlook.com',
            N'45 Hoàng Diệu, Hà Nội', N'Việt Nam', '/uploads/avatars/bich.jpg');
END

IF NOT EXISTS (SELECT 1 FROM Guests WHERE CCCD = N'048200009012')
BEGIN
    INSERT INTO Guests (FullName, CCCD, Phone, Email, Address, Nationality, AvatarUrl)
    VALUES (N'Lê Minh Cường', N'048200009012', N'0923456789', N'cuong.le@gmail.com',
            N'89 Nguyễn Văn Linh, Đà Nẵng', N'Việt Nam', '/uploads/avatars/cuong.jpg');
END

IF NOT EXISTS (SELECT 1 FROM Guests WHERE CCCD = N'A12345678')
BEGIN
    INSERT INTO Guests (FullName, CCCD, Phone, Address, Nationality, AvatarUrl)
    VALUES (N'John Smith', N'A12345678', N'+1-555-0100', N'New York, USA', N'Mỹ', '/uploads/avatars/john.jpg');
END

PRINT N'Đã chạy xong script seed demo.';
