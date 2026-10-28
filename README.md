<div align="center">

# 🎮 GameManagementApi

### Hệ thống Web API quản lý nạp game trực tuyến

*Xây dựng bằng ASP.NET Core · SQL Server · ADO.NET · JWT*

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)

![Architecture](https://img.shields.io/badge/Architecture-Layered-blueviolet?style=flat-square)
![Data Access](https://img.shields.io/badge/Data%20Access-ADO.NET%20%2B%20Stored%20Procedures-orange?style=flat-square)
![Auth](https://img.shields.io/badge/Auth-JWT%20Bearer-success?style=flat-square)
![Status](https://img.shields.io/badge/Status-Ho%C3%A0n%20th%C3%A0nh%20backend-brightgreen?style=flat-square)

</div>

---

## 📑 Mục lục

- [✨ Giới thiệu](#-giới-thiệu)
- [🧰 Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [🏗️ Kiến trúc dự án](#️-kiến-trúc-dự-án)
- [🚀 Chức năng chính](#-chức-năng-chính)
- [🔐 Xác thực & phân quyền](#-xác-thực--phân-quyền)
- [📡 Danh sách API](#-danh-sách-api)
- [💸 Luồng nghiệp vụ đơn hàng & ví](#-luồng-nghiệp-vụ-đơn-hàng--ví)
- [🗄️ Cơ sở dữ liệu](#️-cơ-sở-dữ-liệu)
- [⚙️ Cài đặt & chạy dự án](#️-cài-đặt--chạy-dự-án)
- [🧪 Kiểm thử](#-kiểm-thử)
- [🛡️ Bảo mật & lưu ý triển khai](#️-bảo-mật--lưu-ý-triển-khai)
- [🗺️ Hướng phát triển](#️-hướng-phát-triển)
- [👤 Tác giả](#-tác-giả)

---

## ✨ Giới thiệu

**GameManagementApi** là backend mô phỏng hệ thống bán gói nạp game trực tuyến. Hệ thống cung cấp bộ RESTful API để:

- 👤 Quản lý tài khoản, hồ sơ người dùng và phân quyền theo vai trò
- 🕹️ Quản lý game, gói nạp và chương trình chiết khấu
- 🛒 Tạo đơn hàng, thanh toán bằng **ví** hoặc **chuyển khoản**
- 💰 Nạp tiền vào ví, hoàn tiền, ghi nhận lịch sử giao dịch
- 🔒 Bảo đảm toàn vẹn dữ liệu cho các nghiệp vụ tài chính (giao dịch, khóa dòng, xử lý đồng thời)

### 🎯 Mục tiêu dự án

| # | Mục tiêu |
|---|----------|
| 1 | Xây dựng RESTful API bằng ASP.NET Core |
| 2 | Áp dụng kiến trúc phân tầng, tách biệt trách nhiệm |
| 3 | Dùng SQL Server với **Stored Procedures + ADO.NET** |
| 4 | Xác thực JWT và phân quyền theo vai trò |
| 5 | Kiểm soát tính toàn vẹn dữ liệu trong nghiệp vụ tài chính |

---

## 🧰 Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| 💻 Ngôn ngữ | C# |
| 🌐 Framework | ASP.NET Core Web API (`net10.0`) |
| 🗄️ Cơ sở dữ liệu | Microsoft SQL Server |
| 🔌 Truy cập dữ liệu | ADO.NET (`Microsoft.Data.SqlClient`) |
| 📜 Nghiệp vụ dữ liệu | SQL Stored Procedures |
| 🪪 Xác thực | JWT Bearer Authentication |
| 🔑 Mã hóa mật khẩu | BCrypt |
| 📖 Tài liệu API | Swagger / OpenAPI (Swashbuckle) |
| 🤫 Cấu hình bí mật (dev) | .NET User Secrets |

---

## 🏗️ Kiến trúc dự án

Dự án chia thành 4 project theo mô hình phân tầng:

```text
GameManagementApi/
├── 📦 GameManagement.Domain/            # Lõi nghiệp vụ
│   ├── Entities/                        #   Account, User, Game, GoiNap, DonHang, ChietKhau, LichSuVi
│   └── Enums/                           #   Vai trò & trạng thái
│
├── 📦 GameManagement.Application/       # Logic ứng dụng
│   ├── DTOs/                            #   Dữ liệu vào/ra
│   ├── Interfaces/                      #   Hợp đồng của Service
│   ├── Repositories/                    #   Hợp đồng của Repository
│   ├── Services/                        #   Xử lý nghiệp vụ
│   ├── Exceptions/                      #   Exception theo nghiệp vụ
│   ├── ExceptionHandlers/               #   GlobalExceptionHandler (middleware)
│   └── Common/Converters/               #   JSON converter
│
├── 📦 GameManagement.Infrastructure/    # Hạ tầng
│   ├── Repositories/                    #   ADO.NET + Stored Procedures
│   ├── Data/                            #   SqlConnectionFactory, ApplicationDbContext
│   ├── Services/                        #   JwtService, CurrentUserService
│   ├── BackgroundServices/              #   DonHangExpirationService
│   └── Configurations/                  #   JwtSettings
│
├── 📦 GameManagementApi/                # Tầng trình bày (Web API)
│   ├── Controllers/                     #   Các HTTP endpoint
│   ├── Program.cs                       #   DI, JWT, Swagger, pipeline
│   └── appsettings.json
│
├── 🗄️ Database/                         # Script SQL
│   ├── 01_schema.sql                    #   Bảng, ràng buộc, Stored Procedures
│   ├── 01_schema_test.sql               #   Schema cho database kiểm thử
│   ├── 02_seed.sql                      #   Dữ liệu mẫu (Game, Gói nạp)
│   └── Tests/                           #   Script kiểm thử nghiệp vụ
│
└── 📄 README.md
```

### 🔄 Luồng xử lý một request

```text
Client ──▶ Controller ──▶ Service ──▶ Repository ──▶ Stored Procedure ──▶ SQL Server
              │              │            │
              │              │            └─ ánh xạ lỗi SQL → exception nghiệp vụ
              │              └─ kiểm tra dữ liệu, điều phối nghiệp vụ
              └─ xác thực JWT, phân quyền, trả HTTP status
                         ▲
                GlobalExceptionHandler (bắt mọi lỗi → JSON thống nhất)
```

### 📚 Trách nhiệm từng tầng

- **Domain** – Entity và enum đại diện dữ liệu nghiệp vụ, không phụ thuộc tầng nào khác.
- **Application** – DTO, interface, service xử lý nghiệp vụ, kiểm tra dữ liệu đầu vào, định nghĩa exception.
- **Infrastructure** – Cài đặt repository bằng ADO.NET, gọi Stored Procedure, sinh JWT, chạy tác vụ nền.
- **GameManagementApi** – Controller, cấu hình DI, JWT, Swagger và middleware.

---

## 🚀 Chức năng chính

### 👤 Quản lý tài khoản & người dùng
- Đăng ký, đăng nhập và cấp JWT
- Xem thông tin tài khoản hiện tại, xem/cập nhật hồ sơ cá nhân
- Đổi mật khẩu
- ADMIN: xem danh sách, tạo tài khoản, đổi vai trò/trạng thái, khóa/mở khóa, xóa mềm

### 🕹️ Quản lý game & gói nạp
- Xem danh sách, tìm kiếm, lọc theo trạng thái, phân trang
- ADMIN: tạo, cập nhật, đổi trạng thái (`HOAT_DONG` / `BAO_TRI`), xóa
- Hỗ trợ game có hoặc không có **Server**

### 🏷️ Chiết khấu
- ADMIN: tạo, xem, cập nhật, xóa mềm chương trình chiết khấu cho từng gói nạp

### 🛒 Đơn hàng & thanh toán
- Tạo đơn mua gói nạp, thanh toán bằng **Ví** hoặc **Chuyển khoản**
- Tạo yêu cầu **nạp tiền vào ví** (từ 10.000 đến 1.000.000 VNĐ)
- ADMIN xác nhận chuyển khoản, xác nhận nạp tiền, ghi nhận chuyển khoản **muộn**
- ADMIN/CSKH cập nhật trạng thái xử lý đơn
- Hoàn tiền vào ví hoặc ghi nhận hoàn tiền chuyển khoản trực tiếp
- 🕒 Tác vụ nền tự động **hủy đơn chuyển khoản hết hạn** (kiểm tra mỗi phút)

### 💳 Lịch sử ví
- Người dùng xem lịch sử ví của chính mình
- ADMIN/CSKH tra cứu lịch sử ví của mọi tài khoản
- Lọc theo loại giao dịch, trạng thái, khoảng ngày; có phân trang

---

## 🔐 Xác thực & phân quyền

Hệ thống dùng **JWT Bearer**. Client gửi token trong header:

```http
Authorization: Bearer <access_token>
```

### 🎭 Vai trò

| Vai trò | Mô tả |
|---|---|
| 🙋 `USER` | Người dùng thông thường |
| 🎧 `CSKH` | Nhân viên chăm sóc khách hàng |
| 👑 `ADMIN` | Quản trị viên |

### 🛡️ Cơ chế bảo vệ

- ✅ Kiểm tra Issuer, Audience, thời hạn và chữ ký của token
- ✅ Mỗi request đều **kiểm tra lại tài khoản còn tồn tại và đang hoạt động** — tài khoản bị khóa/xóa sẽ bị từ chối ngay cả khi token chưa hết hạn
- ✅ Người dùng chỉ xem được đơn hàng và lịch sử ví **của chính mình**; ADMIN/CSKH xem được tất cả
- ✅ Ứng dụng từ chối khởi động nếu `Jwt:Key` thiếu hoặc ngắn hơn 32 ký tự

---

## 📡 Danh sách API

> 💡 Toàn bộ endpoint có thể thử trực tiếp trên **Swagger UI** khi chạy ở môi trường Development.

Ký hiệu quyền: 🌍 công khai · 🔓 đã đăng nhập · 🎧 ADMIN/CSKH · 👑 ADMIN

### 🔑 Account — `/api/Account`

| Method | Endpoint | Quyền | Mô tả |
|---|---|:---:|---|
| `POST` | `/register` | 🌍 | Đăng ký tài khoản |
| `POST` | `/login` | 🌍 | Đăng nhập, nhận JWT |
| `GET` | `/me` | 🔓 | Thông tin tài khoản hiện tại |
| `PUT` | `/change-password` | 🔓 | Đổi mật khẩu |
| `GET` | `/admin/all` | 👑 | Danh sách tài khoản (lọc, phân trang) |
| `GET` | `/{id}` | 👑 | Chi tiết tài khoản |
| `POST` | `/admin/create` | 👑 | Tạo tài khoản |
| `PUT` | `/{id}` | 👑 | Cập nhật vai trò/trạng thái |
| `PATCH` | `/{id}/status` | 👑 | Khóa / kích hoạt tài khoản |
| `DELETE` | `/{id}` | 👑 | Xóa (mềm) tài khoản |

### 🧑 User — `/api/User`

| Method | Endpoint | Quyền | Mô tả |
|---|---|:---:|---|
| `GET` | `/me` | 🔓 | Xem hồ sơ của tôi |
| `PUT` | `/me` | 🔓 | Cập nhật hồ sơ (kể cả thông tin ngân hàng) |
| `GET` | `/admin/all` | 👑 | Danh sách người dùng |
| `GET` | `/{id}` | 👑 | Chi tiết hồ sơ |

### 🕹️ Game — `/api/Game`

| Method | Endpoint | Quyền | Mô tả |
|---|---|:---:|---|
| `GET` | `/` | 🔓 | Danh sách game (tìm kiếm, lọc, phân trang) |
| `GET` | `/{id}` | 🔓 | Chi tiết game |
| `POST` | `/` | 👑 | Tạo game |
| `PUT` | `/{id}` | 👑 | Cập nhật game |
| `PATCH` | `/{id}/status` | 👑 | Đổi trạng thái game |
| `DELETE` | `/{id}` | 👑 | Xóa game |

### 📦 GoiNap — `/api/GoiNap`

| Method | Endpoint | Quyền | Mô tả |
|---|---|:---:|---|
| `GET` | `/` | 🔓 | Danh sách gói nạp (lọc theo game, tên, trạng thái) |
| `GET` | `/{id}` | 🔓 | Chi tiết gói nạp |
| `POST` | `/` | 👑 | Tạo gói nạp |
| `PUT` | `/{id}` | 👑 | Cập nhật gói nạp |
| `PATCH` | `/{id}/status` | 👑 | Đổi trạng thái gói nạp |
| `DELETE` | `/{id}` | 👑 | Xóa gói nạp |

### 🏷️ ChietKhau — `/api/ChietKhau`

| Method | Endpoint | Quyền | Mô tả |
|---|---|:---:|---|
| `GET` | `/` | 👑 | Danh sách chiết khấu |
| `GET` | `/{id}` | 👑 | Chi tiết chiết khấu |
| `POST` | `/` | 👑 | Tạo chiết khấu |
| `PUT` | `/{id}` | 👑 | Cập nhật chiết khấu |
| `DELETE` | `/{id}` | 👑 | Xóa mềm chiết khấu |

### 🛒 DonHang — `/api/DonHang`

| Method | Endpoint | Quyền | Mô tả |
|---|---|:---:|---|
| `POST` | `/` | 🔓 | Tạo đơn mua gói nạp |
| `GET` | `/{id}` | 🔓 | Chi tiết đơn (chủ đơn, ADMIN, CSKH) |
| `GET` | `/my-orders` | 🔓 | Lịch sử đơn của tôi |
| `POST` | `/nap-tien` | 🔓 | Tạo yêu cầu nạp tiền vào ví |
| `GET` | `/admin/all` | 🎧 | Danh sách mọi đơn hàng |
| `PATCH` | `/{id}/status` | 🎧 | Cập nhật trạng thái xử lý đơn |
| `POST` | `/{id}/hoan-tien` | 👑 | Hoàn tiền vào ví |
| `POST` | `/{donHangId}/xac-nhan-nap-tien` | 👑 | Xác nhận đã nhận tiền nạp vào ví |
| `POST` | `/{id}/xac-nhan-chuyen-khoan` | 👑 | Xác nhận thanh toán chuyển khoản mua gói |
| `POST` | `/{id}/hoan-tien-chuyen-khoan` | 👑 | Ghi nhận hoàn tiền chuyển khoản trực tiếp |
| `POST` | `/{id}/xac-nhan-chuyen-khoan-muon` | 👑 | Ghi nhận chuyển khoản muộn (đơn đã hết hạn) |

### 💳 LichSuVi — `/api/LichSuVi`

| Method | Endpoint | Quyền | Mô tả |
|---|---|:---:|---|
| `GET` | `/` | 🔓 | Lịch sử ví của tôi |
| `GET` | `/{id}` | 🔓 | Chi tiết giao dịch (USER chỉ xem của mình) |
| `GET` | `/admin` | 🎧 | Tra cứu lịch sử ví mọi tài khoản |

---

## 💸 Luồng nghiệp vụ đơn hàng & ví

### 🛍️ Mua gói nạp

```text
Tạo đơn ──▶ Chọn phương thức thanh toán
              │
              ├─ 💰 VÍ ─────────▶ Trừ số dư ngay ──▶ Đơn chờ xử lý
              │
              └─ 🏦 CHUYỂN KHOẢN ▶ Chờ thanh toán (có hạn)
                                      │
                    ┌─────────────────┼──────────────────┐
                    ▼                 ▼                  ▼
            ADMIN xác nhận     Hết hạn → tự hủy    Hết hạn nhưng khách
            đã nhận tiền       (tác vụ nền)        vẫn chuyển → ADMIN ghi
                                                   nhận "chuyển khoản muộn"
                                                   → cộng tiền vào ví
```

### 📦 Trạng thái xử lý đơn

```text
CHO_XU_LY ──▶ DANG_XU_LY ──▶ HOAN_THANH
                  │
                  └──▶ NAP_GAME_THAT_BAI ──▶ Hoàn tiền (vào ví / chuyển khoản)

HUY  ← đơn chuyển khoản hết hạn
```

### 🧾 Quy tắc đáng chú ý

- 🔁 Mỗi tài khoản chỉ có **một giao dịch chuyển khoản đang chờ** tại một thời điểm
- 🎮 Game có server bắt buộc chọn `Server`; game không có server không được gửi `Server`
- 🛠️ Game đang `BAO_TRI` hoặc gói nạp không hoạt động thì không tạo được đơn
- 💵 Nạp tiền vào ví: từ **10.000** đến **1.000.000 VNĐ** mỗi lần
- 📒 Mọi biến động số dư đều được ghi vào **lịch sử ví** với các loại: `NAP_TIEN_VI`, `THANH_TOAN_DON`, `HOAN_TIEN_DON_LOI`, `CONG_TIEN_ADMIN`, `TRU_TIEN_ADMIN`
- 🔒 Các thao tác tiền được thực hiện trong **transaction** và có khóa dòng để tránh sai lệch số dư khi xử lý đồng thời

---

## 🗄️ Cơ sở dữ liệu

### 📋 Các bảng

| Bảng | Vai trò |
|---|---|
| `Account` | Tài khoản đăng nhập, vai trò, số dư ví, trạng thái |
| `User` | Hồ sơ cá nhân và thông tin ngân hàng |
| `Game` | Danh sách game |
| `GoiNap` | Gói nạp thuộc từng game |
| `ChietKhau` | Chương trình chiết khấu theo gói nạp |
| `DonHang` | Đơn mua gói và yêu cầu nạp tiền |
| `LichSuVi` | Nhật ký biến động số dư |

### 📁 Script SQL

| File | Công dụng |
|---|---|
| `Database/01_schema.sql` | Tạo database, bảng, ràng buộc, index và toàn bộ Stored Procedures |
| `Database/01_schema_test.sql` | Schema dành cho **database kiểm thử** riêng |
| `Database/02_seed.sql` | Dữ liệu mẫu: 3 game và các gói nạp (giá chỉ mang tính minh họa) |
| `Database/Tests/*` | Script kiểm thử nghiệp vụ |

> ⚠️ Seed **không** tạo tài khoản, người dùng hay mật khẩu. Hãy tạo tài khoản ADMIN đầu tiên theo hướng dẫn ở mục [Tạo tài khoản ADMIN đầu tiên](#-tạo-tài-khoản-admin-đầu-tiên).

---

## ⚙️ Cài đặt & chạy dự án

### 📋 Yêu cầu môi trường

- ✅ **.NET SDK 10.0** (khớp `TargetFramework` của project)
- ✅ **SQL Server** hoặc **SQL Server LocalDB**
- ✅ Visual Studio / Rider / VS Code
- ✅ SQL Server Management Studio (SSMS) hoặc công cụ tương đương

### 1️⃣ Lấy source code

```bash
git clone <YOUR_REPOSITORY_URL>
cd GameManagementApi
```

### 2️⃣ Chuẩn bị cơ sở dữ liệu

Chạy lần lượt trong SSMS (hoặc `sqlcmd`):

```text
1. Database/01_schema.sql   →  tạo database, bảng, Stored Procedures
2. Database/02_seed.sql     →  nạp dữ liệu mẫu (tùy chọn)
```

> 📝 Tên database trong script phải khớp với `Database=` trong chuỗi kết nối bên dưới.

### 3️⃣ Cấu hình User Secrets

Tại thư mục chứa `GameManagementApi.csproj`:

```bash
cd GameManagementApi
dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<YOUR_CONNECTION_STRING>"
dotnet user-secrets set "Jwt:Key" "<YOUR_SECRET_KEY_TOI_THIEU_32_KY_TU>"
```

Ví dụ chuỗi kết nối với LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=GameManagementDB;Trusted_Connection=True;TrustServerCertificate=True;
```

> 🔐 `Jwt:Key` phải có **ít nhất 32 ký tự**, nếu không ứng dụng sẽ không khởi động. **Không commit** các giá trị bí mật lên Git.

### 4️⃣ Kiểm tra `appsettings.json`

File này chỉ chứa cấu hình **không nhạy cảm**:

```json
{
  "Jwt": {
    "Issuer": "GameManagementApi",
    "Audience": "GameManagementClient",
    "ExpireMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 5️⃣ Build

```bash
dotnet build GameManagementApi.slnx -c Release
```

### 6️⃣ Chạy API

```bash
cd GameManagementApi
dotnet run
```

Mở Swagger UI theo địa chỉ hiển thị trong terminal, ví dụ:

```text
http://localhost:5193/swagger
```

### 🔑 Dùng JWT trên Swagger

1. Gọi `POST /api/Account/login` để lấy `token`
2. Bấm nút **Authorize** 🔒 ở góc phải Swagger UI
3. Dán token vào ô nhập, sau đó gọi các endpoint cần xác thực

### 👑 Tạo tài khoản ADMIN đầu tiên

Endpoint `register` luôn tạo tài khoản vai trò `USER`. Để có ADMIN đầu tiên:

1. Đăng ký một tài khoản bằng `POST /api/Account/register`
2. Trong SSMS, nâng quyền tài khoản đó:

```sql
UPDATE dbo.Account
SET Role = 'ADMIN'
WHERE TenDangNhap = N'<ten_dang_nhap_cua_ban>';
```

3. Đăng nhập lại để nhận token mới có vai trò ADMIN. Từ đó có thể dùng `POST /api/Account/admin/create` để tạo thêm ADMIN/CSKH.

---

## 🧪 Kiểm thử

Dự án được kiểm thử thủ công qua **Swagger** và bằng các **script SQL** chạy trên database kiểm thử riêng.

### ✅ Nhóm kiểm thử đã thực hiện

- Đăng ký, đăng nhập, xác thực JWT
- Lấy thông tin tài khoản hiện tại
- Phân quyền `USER` / `CSKH` / `ADMIN`
- API quản lý game, gói nạp, tài khoản, đơn hàng
- Nghiệp vụ SQL về thanh toán ví, hoàn tiền, xác nhận giao dịch
- Cập nhật đồng thời liên quan đến giao dịch và số dư

### 🧾 Các script trong `Database/Tests/`

| Script | Kiểm tra |
|---|---|
| `01_Test_NapTienThanhCong.sql` | Nạp tiền vào ví thành công |
| `02_Test_XacNhanLap.sql` | Xác nhận nạp tiền lặp lại không cộng tiền hai lần |
| `03_Test_NapTien_DauVao.sql` | Kiểm tra dữ liệu đầu vào khi nạp tiền |
| `04_Test_HuyDonHetHan.sql` | Tự động hủy đơn chuyển khoản hết hạn |
| `05_Test_HoanTienLap.sql` | Hoàn tiền lặp lại không hoàn hai lần |
| `06_Test_ThanhToanVi.sql` | Thanh toán đơn bằng số dư ví |
| `07_Test_ChuyenKhoanMuon.sql` | Ghi nhận chuyển khoản muộn |
| `08_Test_DongThoi.ps1` | Kịch bản gọi **đồng thời** để kiểm tra số dư không sai lệch |

> ⚠️ Chạy test trên **database kiểm thử** (`01_schema_test.sql`), tuyệt đối không chạy trên database thật. Đảm bảo dữ liệu đầu vào đúng yêu cầu của từng script trước khi chạy lại.

---

## 🛡️ Bảo mật & lưu ý triển khai

- 🔒 Không lưu mật khẩu hoặc `Jwt:Key` trong source code; dùng **User Secrets** khi phát triển và **biến môi trường** khi triển khai
- 🧱 Không dùng database kiểm thử cho production
- 🧂 Mật khẩu được băm bằng **BCrypt**, không bao giờ trả về trong response
- 🚧 Lỗi hệ thống được che bằng thông báo chung, chi tiết chỉ ghi vào log
- 🪵 Không ghi thông tin cá nhân hoặc bí mật vào log
- 📖 Swagger chỉ bật ở môi trường **Development**
- ⏳ Vai trò được lưu trong JWT, nên khi đổi vai trò, token cũ giữ vai trò cũ cho tới khi hết hạn (mặc định 60 phút); trạng thái hoạt động của tài khoản thì được kiểm tra lại ở mỗi request
- ✅ Kiểm thử lại các nghiệp vụ quan trọng sau mỗi lần thay đổi database hoặc Stored Procedures

---

## 🗺️ Hướng phát triển

- [ ] Giới hạn tần suất request cho `login` / `register` (rate limiting)
- [ ] Cấu hình CORS khi kết nối với frontend
- [ ] Viết unit test / integration test bằng xUnit
- [ ] Refresh token và thu hồi token
- [ ] Tích hợp cổng thanh toán tự động thay cho xác nhận thủ công
- [ ] Đóng gói bằng Docker và thiết lập CI/CD

---

## 👤 Tác giả

Dự án cá nhân phục vụ mục đích học tập và thực hành phát triển backend với **C#**, **ASP.NET Core Web API** và **SQL Server**.

<div align="center">

⭐ Nếu thấy dự án hữu ích, hãy để lại một **star** nhé! ⭐

</div>
