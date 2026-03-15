// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Areas.Identity.Pages.Account
{
    /// <summary>
    /// PageModel xử lý luồng tài khoản 'Login.cshtml'.
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        /// Khởi tạo lớp LoginModel và nạp các dependency cần thiết.
        /// </summary>
        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
            /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
            /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
            /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
            /// </summary>
            [Display(Name = "Ghi nhớ đăng nhập?")]
            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Xử lý yêu cầu GET để nạp dữ liệu và hiển thị trang.
        /// </summary>
        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Xóa cookie đăng nhập ngoài hiện tại để đảm bảo quy trình đăng nhập mới sạch trạng thái
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Xử lý yêu cầu POST, kiểm tra dữ liệu đầu vào và lưu thay đổi.
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Lần đăng nhập thất bại này không được tính vào cơ chế khóa tài khoản
                // Đặt lockoutOnFailure = true nếu muốn tự động khóa tài khoản khi sai mật khẩu nhiều lần
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Đăng nhập không thành công. Vui lòng kiểm tra lại email và mật khẩu.");
                    return Page();
                }
            }

            // Nếu chạy tới đây nghĩa là có lỗi, hiển thị lại biểu mẫu cho người dùng chỉnh sửa
            return Page();
        }
    }
}





