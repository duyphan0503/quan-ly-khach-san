// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace HotelManagement.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// PageModel xử lý luồng tài khoản 'Email.cshtml'.
    /// </summary>
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Khởi tạo lớp EmailModel và nạp các dependency cần thiết.
        /// </summary>
        public EmailModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
            [Required(ErrorMessage = "Vui lòng nhập Email mới.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            [Display(Name = "Email mới")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        /// <summary>
        /// Xử lý yêu cầu GET để nạp dữ liệu và hiển thị trang.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        /// <summary>
        /// Thực hiện nghiệp vụ của miền input.
        /// </summary>
        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "Xác nhận email của bạn",
                    $"Vui lòng xác nhận tài khoản của bạn bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>nhấn vào đây</a>.");

                StatusMessage = "Liên kết xác nhận thay đổi email đã được gửi. Vui lòng kiểm tra hộp thư của bạn.";
                return RedirectToPage();
            }

            StatusMessage = "Email của bạn không thay đổi.";
            return RedirectToPage();
        }

        /// <summary>
        /// Thực hiện nghiệp vụ của miền input.
        /// </summary>
        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Xác nhận email của bạn",
                $"Vui lòng xác nhận tài khoản của bạn bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>nhấn vào đây</a>.");

            StatusMessage = "Email xác nhận đã được gửi. Vui lòng kiểm tra hộp thư của bạn.";
            return RedirectToPage();
        }
    }
}






