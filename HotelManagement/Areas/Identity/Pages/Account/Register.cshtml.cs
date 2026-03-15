// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Areas.Identity.Pages.Account
{
    /// <summary>
    /// PageModel xử lý luồng tài khoản 'Register.cshtml'.
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IGuestService _guestService;

        /// <summary>
        /// Khởi tạo lớp RegisterModel và nạp các dependency cần thiết.
        /// </summary>
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IGuestService guestService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _guestService = guestService;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
            [Required(ErrorMessage = "Vui lòng nhập họ tên")]
            [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự.")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập Email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [StringLength(100, ErrorMessage = "{0} phải từ {2} đến {1} ký tự.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }
        }



        /// <summary>
        /// Xử lý yêu cầu GET để nạp dữ liệu và hiển thị trang.
        /// </summary>
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
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
                var normalizedPhone = Input.PhoneNumber?.Trim();
                var existingUserByPhone = await _userManager.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone);
                if (existingUserByPhone != null)
                {
                    ModelState.AddModelError(string.Empty, "Số điện thoại này đã được dùng cho một tài khoản khác.");
                    return Page();
                }

                var existingGuest = await _guestService.SearchByPhoneOrCCCDAsync(normalizedPhone);
                if (!string.IsNullOrWhiteSpace(existingGuest?.UserId))
                {
                    ModelState.AddModelError(string.Empty, "Số điện thoại này đã liên kết với một tài khoản khác.");
                    return Page();
                }

                var user = CreateUser();
                user.FullName = Input.FullName;
                user.PhoneNumber = normalizedPhone;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);

                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    (bool Success, string Message) guestSyncResult;
                    if (existingGuest != null)
                    {
                        existingGuest.UserId = user.Id;
                        existingGuest.FullName = Input.FullName;
                        existingGuest.Email = Input.Email;
                        existingGuest.PhoneNumber = normalizedPhone;
                        guestSyncResult = await _guestService.UpdateAsync(existingGuest);
                    }
                    else
                    {
                        guestSyncResult = await _guestService.CreateAsync(new Guest
                        {
                            FullName = Input.FullName,
                            Email = Input.Email,
                            PhoneNumber = normalizedPhone,
                            UserId = user.Id,
                            AvatarUrl = user.AvatarUrl
                        });
                    }

                    if (!guestSyncResult.Success)
                    {
                        await _userManager.DeleteAsync(user);
                        ModelState.AddModelError(string.Empty, guestSyncResult.Message);
                        return Page();
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Xác nhận email của bạn",
                        $"Vui lòng xác nhận tài khoản của bạn bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>nhấn vào đây</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Nếu chạy tới đây nghĩa là có lỗi, hiển thị lại biểu mẫu cho người dùng chỉnh sửa
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}