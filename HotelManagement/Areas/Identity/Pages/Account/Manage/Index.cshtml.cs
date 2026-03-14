// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Caching.Memory;

namespace HotelManagement.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IGuestService _guestService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment, IGuestService guestService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
            _guestService = guestService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Vui lòng nhập họ tên")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Ảnh đại diện")]
            public IFormFile AvatarFile { get; set; }

            public string AvatarUrl { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FullName = user.FullName,
                PhoneNumber = phoneNumber,
                AvatarUrl = user.AvatarUrl
            };
        }


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

        public async Task<IActionResult> OnPostAsync()
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

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            bool isGuestUpdated = false;
            var guest = await _guestService.GetByUserIdAsync(user.Id);

            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Lỗi khi cập nhật thông tin cá nhân.";
                    return RedirectToPage();
                }

                if (guest != null)
                {
                    guest.FullName = Input.FullName;
                    isGuestUpdated = true;
                }
            }

            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Lỗi không xác định khi cập nhật số điện thoại.";
                    return RedirectToPage();
                }

                if (guest != null)
                {
                    guest.PhoneNumber = Input.PhoneNumber;
                    isGuestUpdated = true;
                }
            }

            // Xử lý upload ảnh đại diện
            if (Input.AvatarFile != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(Input.AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.AvatarFile.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, user.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath) && !oldFilePath.Contains("default"))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    user.AvatarUrl = $"/uploads/avatars/{fileName}";
                    await _userManager.UpdateAsync(user);

                    if (guest != null)
                    {
                        guest.AvatarUrl = user.AvatarUrl;
                        isGuestUpdated = true;
                    }
                }
                catch (Exception)
                {
                    StatusMessage = "Error: Không thể tải ảnh lên. Vui lòng thử lại.";
                    return RedirectToPage();
                }
            }

            if (isGuestUpdated && guest != null)
            {
                await _guestService.UpdateAsync(guest);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Hồ sơ của bạn đã được cập nhật thành công.";
            return RedirectToPage();
        }
    }
}
