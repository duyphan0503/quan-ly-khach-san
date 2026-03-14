using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Infrastructure.Identity
{
    public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() => new IdentityError { Code = nameof(DefaultError), Description = "Đã xảy ra lỗi không xác định." };
        public override IdentityError ConcurrencyFailure() => new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Lỗi không nhất quán dữ liệu, thực thể đã được thay đổi." };
        public override IdentityError PasswordMismatch() => new IdentityError { Code = nameof(PasswordMismatch), Description = "Mật khẩu không chính xác." };
        public override IdentityError InvalidToken() => new IdentityError { Code = nameof(InvalidToken), Description = "Mã xác thực không hợp lệ." };
        public override IdentityError LoginAlreadyAssociated() => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Người dùng đã có tài khoản liên kết." };
        public override IdentityError InvalidUserName(string? userName) => new IdentityError { Code = nameof(InvalidUserName), Description = $"Tên đăng nhập '{userName}' không hợp lệ, chỉ được chứa chữ cái và số." };
        public override IdentityError InvalidEmail(string? email) => new IdentityError { Code = nameof(InvalidEmail), Description = $"Email '{email}' không hợp lệ." };
        public override IdentityError DuplicateUserName(string? userName) => new IdentityError { Code = nameof(DuplicateUserName), Description = $"Tên đăng nhập '{userName}' đã tồn tại." };
        public override IdentityError DuplicateEmail(string? email) => new IdentityError { Code = nameof(DuplicateEmail), Description = $"Email '{email}' đã được sử dụng." };
        public override IdentityError InvalidRoleName(string? role) => new IdentityError { Code = nameof(InvalidRoleName), Description = $"Tên vai trò '{role}' không hợp lệ." };
        public override IdentityError DuplicateRoleName(string? role) => new IdentityError { Code = nameof(DuplicateRoleName), Description = $"Vai trò '{role}' đã tồn tại." };
        public override IdentityError UserAlreadyInRole(string? role) => new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"Người dùng đã có vai trò '{role}'." };
        public override IdentityError UserNotInRole(string? role) => new IdentityError { Code = nameof(UserNotInRole), Description = $"Người dùng không có vai trò '{role}'." };
        public override IdentityError PasswordTooShort(int length) => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Mật khẩu phải dài ít nhất {length} ký tự." };
        public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric) , Description = "Mật khẩu phải chứa ít nhất một ký tự đặc biệt." };
        public override IdentityError PasswordRequiresDigit() => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Mật khẩu phải chứa ít nhất một chữ số ('0'-'9')." };
        public override IdentityError PasswordRequiresLower() => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Mật khẩu phải chứa ít nhất một chữ thường ('a'-'z')." };
        public override IdentityError PasswordRequiresUpper() => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Mật khẩu phải chứa ít nhất một chữ hoa ('A'-'Z')." };
    }
}
