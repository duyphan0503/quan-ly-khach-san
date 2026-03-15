// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace HotelManagement.Areas.Identity.Pages.Account
{
    /// <summary>
    /// PageModel xử lý luồng tài khoản 'ForgotPassword.cshtml'.
    /// </summary>
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Khởi tạo lớp ForgotPasswordModel và nạp các dependency cần thiết.
        /// </summary>
        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
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
        public class InputModel
        {
            /// <summary>
            /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
            /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        /// <summary>
        /// Xử lý yêu cầu POST, kiểm tra dữ liệu đầu vào và lưu thay đổi.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Không tiết lộ việc người dùng không tồn tại hoặc chưa xác thực tài khoản
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Để biết thêm cách bật xác thực tài khoản và đặt lại mật khẩu, vui lòng
                // truy cập https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}






