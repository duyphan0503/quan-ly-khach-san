// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Areas.Identity.Pages.Account
{
    /// <summary>
    /// PageModel xử lý luồng tài khoản 'Logout.cshtml'.
    /// </summary>
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        /// <summary>
        /// Khởi tạo lớp LogoutModel và nạp các dependency cần thiết.
        /// </summary>
        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Xử lý yêu cầu POST đồng bộ cho thao tác biểu mẫu.
        /// </summary>
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Cần dùng redirect để trình duyệt thực hiện một yêu cầu mới
                // và cập nhật lại thông tin định danh của người dùng.
                return RedirectToPage();
            }
        }
    }
}

