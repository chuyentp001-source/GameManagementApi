# 🎮 GameManagementApi

Hệ thống Web API quản lý game, gói nạp và xử lý đơn hàng/ví người dùng được xây dựng trên nền tảng **.NET 10** áp dụng kiến trúc **Clean Architecture** kết hợp **Generic Repository** và **Unit of Work**.

---

## 🏗️ Kiến trúc dự án (Project Architecture)

Dự án được phân tách thành 4 lớp độc lập theo chuẩn **Clean Architecture**:

- 📂 **`GameManagement.Domain`**: Chứa các Entities (Account, User, Game, GoiNap, ChietKhau, DonHang, LichSuVi), Enums và Value Objects sinh ra từ CSDL.
- 📂 **`GameManagement.Application`**: Chứa DTOs, Interfaces (Repository, UnitOfWork) và các Services xử lý logic nghiệp vụ.
- 📂 **`GameManagement.Infrastructure`**: Chứa `DbContext` được Scaffold từ SQL Server (Database-First), cấu hình Entity Framework Core và các Repositories triển khai.
- 📂 **`GameManagementApi`**: Lớp Web API chính chứa Controllers, Middlewares, Dependency Injection setup và file cấu hình `appsettings.json`.

---

## 🛠️ Công nghệ sử dụng (Tech Stack)

- **Language & Framework**: C#, .NET 10 Web API
- **Database**: SQL Server
- **ORM & Approach**: Entity Framework Core (Database-First)
- **Architecture**: Clean Architecture
- **Design Patterns**: Generic Repository, Unit of Work, Dependency Injection
- **Documentation**: Swagger / OpenAPI

---

## 🗄️ Mô hình CSDL & Các chức năng chính (Database & Features)

Hệ thống bao gồm **7 bảng CSDL chính**:

1. 🔐 **Account & User**:
   - `Account`: Quản lý tài khoản đăng nhập, mật khẩu, phân quyền (Role) và trạng thái.
   - `User`: Quản lý thông tin chi tiết người dùng (Họ tên, Email, Số điện thoại, Địa chỉ...).
2. 🎮 **Game & GoiNap**:
   - `Game`: Danh sách các tựa game trong hệ thống.
   - `GoiNap`: Quản lý các gói nạp theo từng game và giá cả.
3. 🏷️ **ChietKhau**:
   - Quản lý tỷ lệ phần trăm chiết khấu áp dụng cho các gói nạp theo khoảng thời gian.
4. 🛒 **DonHang & LichSuVi**:
   - `DonHang`: Quản lý đơn hàng nạp game (Thông tin tài khoản game, server, phương thức thanh toán, trạng thái đơn...).
   - `LichSuVi`: Ghi lại lịch sử giao dịch và biến động số dư ví của người dùng.

---

## 🚀 Hướng dẫn cài đặt và khởi chạy (Getting Started)

### 1. Yêu cầu hệ thống (Prerequisites)
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server Management Studio (SSMS) / SQL Server LocalDB

### 2. Khởi tạo Cơ sở dữ liệu (Database Setup)
Thực thi file kịch bản SQL (`Database.sql`) vào SQL Server để khởi tạo CSDL `GameManagementDb` với đầy đủ 7 bảng trên.

### 3. Cấu hình chuỗi kết nối
Mở file `appsettings.json` trong project `GameManagementApi` và cập nhật lại chuỗi kết nối:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=GameManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
