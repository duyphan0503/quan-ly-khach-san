using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Infrastructure.Identity
{
    /// <summary>
    /// Cung cấp bộ thông điệp lỗi tiếng Việt cho ASP.NET Core Identity,
    /// giúp người dùng cuối nhận thông báo rõ ràng và nhất quán khi xác thực/tạo tài khoản.
    /// </summary>
    public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
    {
        /// <summary>
        /// Lỗi mặc định khi hệ thống không xác định được nguyên nhân cụ thể.
        /// </summary>
        public override IdentityError DefaultError() => new IdentityError { Code = nameof(DefaultError), Description = "Đã xảy ra lỗi không xác định." };
        /// <summary>
        /// Lỗi xung đột đồng thời khi bản ghi đã bị thay đổi bởi tiến trình khác.
        /// </summary>
        public override IdentityError ConcurrencyFailure() => new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Lỗi không nhất quán dữ liệu, thực thể đã được thay đổi." };
        /// <summary>
        /// Lỗi khi mật khẩu nhập vào không khớp mật khẩu hiện tại trong hệ thống.
        /// </summary>
        public override IdentityError PasswordMismatch() => new IdentityError { Code = nameof(PasswordMismatch), Description = "Mật khẩu không chính xác." };
        /// <summary>
        /// Lỗi token xác thực không hợp lệ hoặc đã hết hiệu lực.
        /// </summary>
        public override IdentityError InvalidToken() => new IdentityError { Code = nameof(InvalidToken), Description = "Mã xác thực không hợp lệ." };
        /// <summary>
        /// Lỗi khi thông tin đăng nhập ngoài đã được liên kết với tài khoản khác.
        /// </summary>
        public override IdentityError LoginAlreadyAssociated() => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Người dùng đã có tài khoản liên kết." };
        /// <summary>
        /// Lỗi tên đăng nhập không đạt quy tắc định dạng cho phép.
        /// </summary>
        public override IdentityError InvalidUserName(string? userName) => new IdentityError { Code = nameof(InvalidUserName), Description = $"Tên đăng nhập '{userName}' không hợp lệ, chỉ được chứa chữ cái và số." };
        /// <summary>
        /// Lỗi định dạng email không hợp lệ theo chuẩn Identity.
        /// </summary>
        public override IdentityError InvalidEmail(string? email) => new IdentityError { Code = nameof(InvalidEmail), Description = $"Email '{email}' không hợp lệ." };
        /// <summary>
        /// Lỗi trùng tên đăng nhập do đã tồn tại người dùng khác.
        /// </summary>
        public override IdentityError DuplicateUserName(string? userName) => new IdentityError { Code = nameof(DuplicateUserName), Description = $"Tên đăng nhập '{userName}' đã tồn tại." };
        /// <summary>
        /// Lỗi trùng email do địa chỉ đã được gắn với tài khoản khác.
        /// </summary>
        public override IdentityError DuplicateEmail(string? email) => new IdentityError { Code = nameof(DuplicateEmail), Description = $"Email '{email}' đã được sử dụng." };
        /// <summary>
        /// Lỗi tên vai trò không hợp lệ theo quy tắc định danh role.
        /// </summary>
        public override IdentityError InvalidRoleName(string? role) => new IdentityError { Code = nameof(InvalidRoleName), Description = $"Tên vai trò '{role}' không hợp lệ." };
        /// <summary>
        /// Lỗi trùng tên vai trò khi role đã tồn tại trong hệ thống.
        /// </summary>
        public override IdentityError DuplicateRoleName(string? role) => new IdentityError { Code = nameof(DuplicateRoleName), Description = $"Vai trò '{role}' đã tồn tại." };
        /// <summary>
        /// Lỗi khi gán vai trò đã có sẵn cho cùng một người dùng.
        /// </summary>
        public override IdentityError UserAlreadyInRole(string? role) => new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"Người dùng đã có vai trò '{role}'." };
        /// <summary>
        /// Lỗi khi thao tác với vai trò mà người dùng hiện chưa được gán.
        /// </summary>
        public override IdentityError UserNotInRole(string? role) => new IdentityError { Code = nameof(UserNotInRole), Description = $"Người dùng không có vai trò '{role}'." };
        /// <summary>
        /// Lỗi mật khẩu không đạt độ dài tối thiểu theo policy.
        /// </summary>
        public override IdentityError PasswordTooShort(int length) => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Mật khẩu phải dài ít nhất {length} ký tự." };
        /// <summary>
        /// Lỗi mật khẩu thiếu ký tự đặc biệt theo chính sách bảo mật.
        /// </summary>
        public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Mật khẩu phải chứa ít nhất một ký tự đặc biệt." };
        /// <summary>
        /// Lỗi mật khẩu thiếu chữ số theo chính sách bảo mật.
        /// </summary>
        public override IdentityError PasswordRequiresDigit() => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Mật khẩu phải chứa ít nhất một chữ số ('0'-'9')." };
        /// <summary>
        /// Lỗi mật khẩu thiếu chữ thường theo chính sách bảo mật.
        /// </summary>
        public override IdentityError PasswordRequiresLower() => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Mật khẩu phải chứa ít nhất một chữ thường ('a'-'z')." };
        /// <summary>
        /// Lỗi mật khẩu thiếu chữ hoa theo chính sách bảo mật.
        /// </summary>
        public override IdentityError PasswordRequiresUpper() => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Mật khẩu phải chứa ít nhất một chữ hoa ('A'-'Z')." };
    }
}

