// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace  HotelManagement.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
    /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
    /// </summary>
    public static class ManageNavPages
    {
        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string Index => "Index";

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string Email => "Email";

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string ChangePassword => "ChangePassword";

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string DownloadPersonalData => "DownloadPersonalData";

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string DeletePersonalData => "DeletePersonalData";

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string ExternalLogins => "ExternalLogins";

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string PersonalData => "PersonalData";



        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string DownloadPersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DownloadPersonalData);

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string DeletePersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeletePersonalData);

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);

        /// <summary>
        /// API này phục vụ hạ tầng giao diện mặc định của ASP.NET Core Identity.
        /// Không khuyến nghị gọi trực tiếp từ mã nghiệp vụ và có thể thay đổi ở các phiên bản sau.
        /// </summary>
        public static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}





