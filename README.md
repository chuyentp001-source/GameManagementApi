GameManagementApi

Web API quản lý hệ thống nạp game, được xây dựng bằng C# và ASP.NET Core Web API. Dự án hỗ trợ quản lý tài khoản, trò chơi, gói nạp, đơn hàng và các nghiệp vụ thanh toán liên quan.

1. Giới thiệu dự án

GameManagementApi là dự án backend mô phỏng hệ thống bán gói nạp game trực tuyến. Hệ thống cung cấp API để quản lý dữ liệu, xác thực người dùng, phân quyền truy cập và xử lý các nghiệp vụ liên quan đến đơn hàng và số dư ví.

Mục tiêu của dự án:

Xây dựng RESTful API bằng ASP.NET Core.

Áp dụng kiến trúc phân tầng để tách biệt trách nhiệm.

Sử dụng SQL Server để lưu trữ dữ liệu.

Sử dụng Stored Procedures và ADO.NET để truy vấn, cập nhật dữ liệu.

Xác thực bằng JWT và phân quyền theo vai trò.

Kiểm soát tính toàn vẹn dữ liệu trong các nghiệp vụ tài chính.

2. Công nghệ sử dụng

Ngôn ngữ: C#

Backend: ASP.NET Core Web API

Cơ sở dữ liệu: Microsoft SQL Server

Truy cập dữ liệu: ADO.NET

Truy vấn nghiệp vụ: SQL Stored Procedures

Xác thực: JWT Bearer Authentication

Mã hóa mật khẩu: BCrypt

Kiểm thử API: Swagger / OpenAPI

Quản lý cấu hình bí mật khi phát triển: .NET User Secrets

3. Kiến trúc dự án

Dự án được tổ chức thành các project riêng biệt nhằm phân tách trách nhiệm giữa các thành phần.

GameManagementApi/
├── GameManagement.Domain/
│   └── Entities, Enums
│
├── GameManagement.Application/
│   ├── DTOs
│   ├── Interfaces
│   ├── Services
│   └── Exceptions
│
├── GameManagement.Infrastructure/
│   ├── Repositories
│   ├── Configurations
│   └── Data Access
│
├── GameManagementApi/
│   ├── Controllers
│   ├── Middleware
│   ├── Program.cs
│   ├── appsettings.json
│   └── Properties/
│
└── README.md

Trách nhiệm của từng layer

Domain

Chứa các entity đại diện cho dữ liệu nghiệp vụ.

Định nghĩa các enum về vai trò và trạng thái.

Chứa những thành phần cốt lõi của domain.

Application

Chứa DTO dùng để nhận dữ liệu đầu vào và trả dữ liệu đầu ra.

Định nghĩa interface cho service và repository.

Xử lý logic nghiệp vụ thông qua các service.

Kiểm tra dữ liệu và điều phối các thao tác nghiệp vụ.

Infrastructure

Triển khai repository.

Thực hiện truy vấn và cập nhật SQL Server bằng ADO.NET.

Gọi Stored Procedures để xử lý dữ liệu.

GameManagementApi

Cung cấp các HTTP endpoint thông qua controller.

Cấu hình dependency injection, JWT authentication và middleware.

Trả kết quả HTTP cho client.

4. Chức năng chính

Quản lý tài khoản

Đăng ký tài khoản.

Đăng nhập và cấp JWT.

Lấy thông tin tài khoản hiện tại.

Lấy danh sách và thông tin tài khoản.

Cập nhật thông tin được phép thay đổi.

Đổi mật khẩu.

Quản lý vai trò và trạng thái tài khoản.

Quản lý game và gói nạp

Xem danh sách game và thông tin game.

Tạo và cập nhật thông tin game.

Xem danh sách gói nạp.

Tạo và cập nhật gói nạp.

Kiểm tra thông tin gói nạp và các dữ liệu liên quan trước khi tạo đơn hàng.

Quản lý đơn hàng và thanh toán

Tạo đơn hàng.

Kiểm tra tính hợp lệ của dữ liệu đơn hàng.

Thanh toán bằng số dư ví.

Xử lý xác nhận thanh toán và nạp tiền theo trạng thái đơn hàng.

Xử lý các trường hợp nạp game thất bại và hoàn tiền theo điều kiện nghiệp vụ.

Ghi nhận lịch sử giao dịch ví.

Kiểm soát các thao tác đồng thời để hạn chế cập nhật số dư sai lệch.

5. Xác thực và phân quyền

Hệ thống sử dụng JWT Bearer Authentication để xác thực request.

Các vai trò chính:

USER: người dùng thông thường.

CSKH: nhân viên chăm sóc khách hàng.

ADMIN: quản trị viên.

Các API được bảo vệ bằng cơ chế xác thực và phân quyền phù hợp với từng chức năng.

Đối với endpoint yêu cầu đăng nhập, client cần gửi access token trong HTTP header:

Authorization: Bearer <access_token>

Không nên đưa JWT Key vào source code hoặc repository công khai.

6. Cách cài đặt và chạy dự án

Yêu cầu môi trường

.NET SDK tương thích với Target Framework của solution.

SQL Server hoặc SQL Server LocalDB.

Visual Studio hoặc IDE hỗ trợ .NET.

SQL Server Management Studio (SSMS), hoặc công cụ tương đương.

Bước 1: Lấy source code

Clone repository:

git clone <YOUR_REPOSITORY_URL>

Di chuyển vào thư mục dự án:

cd GameManagementApi

Thay <YOUR_REPOSITORY_URL> bằng URL repository thực tế của dự án.

Bước 2: Chuẩn bị cơ sở dữ liệu

Tạo database SQL Server theo cấu hình của môi trường chạy.

Tạo các bảng và quan hệ cần thiết.

Tạo các Stored Procedures được repository sử dụng.

Đảm bảo dữ liệu ban đầu đáp ứng các khóa ngoại và điều kiện nghiệp vụ.

Nếu sử dụng database kiểm thử, hãy cấu hình chuỗi kết nối trỏ đúng database kiểm thử.

Lưu ý: Cần sử dụng các script SQL thực tế của dự án để tạo schema và Stored Procedures. README này không thay thế các script khởi tạo database.

Bước 3: Cấu hình User Secrets

Tại thư mục chứa file project GameManagementApi.csproj, chạy:

dotnet user-secrets init

Thiết lập chuỗi kết nối:

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<YOUR_CONNECTION_STRING>"

Thiết lập JWT Key:

dotnet user-secrets set "Jwt:Key" "<YOUR_SECRET_KEY>"

Thay các giá trị mẫu bằng thông tin phù hợp với môi trường của bạn. JWT Key cần đáp ứng yêu cầu độ dài mà ứng dụng kiểm tra.

Không commit các giá trị bí mật lên Git.

Nếu User Secrets đã được thiết lập trong project thì không cần khởi tạo lại; chỉ cập nhật các giá trị khi cần thiết.

Bước 4: Kiểm tra cấu hình

File appsettings.json chứa những cấu hình không nhạy cảm, ví dụ:

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

Đảm bảo chuỗi kết nối và JWT Key được cung cấp từ nguồn cấu hình phù hợp, chẳng hạn User Secrets khi phát triển hoặc biến môi trường khi triển khai.

Bước 5: Build dự án

Tại thư mục chứa solution, chạy lệnh tương ứng với file solution thực tế:

dotnet build <YOUR_SOLUTION_FILE> -c Release

Thay <YOUR_SOLUTION_FILE> bằng tên file .sln hoặc .slnx của dự án.

Bước 6: Chạy API

Di chuyển đến thư mục chứa GameManagementApi.csproj, sau đó chạy:

dotnet run

Khi ứng dụng khởi động thành công, mở địa chỉ Swagger được hiển thị trong Terminal, ví dụ:

http://localhost:5193/swagger

Địa chỉ thực tế phụ thuộc vào cấu hình chạy của môi trường.

7. Kiểm thử

Dự án đã được kiểm thử thủ công qua Swagger và các script SQL.

Các nhóm kiểm thử đã thực hiện:

Đăng ký, đăng nhập và xác thực JWT.

Lấy thông tin tài khoản hiện tại.

Kiểm tra phân quyền USER/ADMIN.

Kiểm thử các API quản lý game, gói nạp, tài khoản và đơn hàng.

Kiểm thử các nghiệp vụ SQL về thanh toán ví, hoàn tiền và xác nhận giao dịch.

Kiểm thử các tình huống cập nhật đồng thời liên quan đến giao dịch và số dư.

Các script SQL kiểm thử được thực hiện trên database kiểm thử riêng. Cần bảo đảm database và dữ liệu đầu vào đúng với yêu cầu của từng script trước khi chạy lại.

8. Nguyên tắc triển khai

Không lưu mật khẩu hoặc JWT Key trực tiếp trong source code.

Không sử dụng database kiểm thử cho môi trường production.

Kiểm tra dữ liệu đầu vào và quyền truy cập tại các endpoint phù hợp.

Sử dụng Stored Procedures và ADO.NET theo định hướng của dự án.

Bảo đảm thao tác cập nhật số dư và trạng thái giao dịch được thực hiện an toàn.

Không đưa dữ liệu cá nhân hoặc thông tin bí mật vào log không cần thiết.

Kiểm thử lại các nghiệp vụ quan trọng sau khi thay đổi database hoặc Stored Procedures.

9. Trạng thái dự án

Các API chính đã được kiểm thử qua Swagger. Tám bài kiểm thử SQL đã chạy thành công trên database kiểm thử, và solution đã build thành công ở cấu hình Release.

README này mô tả cách thiết lập và chạy dự án. Trước khi triển khai production, cần xác nhận cấu hình môi trường, bảo mật, schema database và các Stored Procedures tương ứng.

10. Tác giả

Dự án cá nhân phục vụ mục đích học tập và thực hành phát triển backend với C#, ASP.NET Core Web API và SQL Server.
