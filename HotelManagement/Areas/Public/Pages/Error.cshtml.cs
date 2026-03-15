using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Public.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
/// <summary>
/// PageModel xử lý trang công khai 'Error.cshtml'.
/// </summary>
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public new int? StatusCode { get; set; }

    private readonly ILogger<ErrorModel> _logger;

    /// <summary>
    /// Khởi tạo lớp ErrorModel và nạp các dependency cần thiết.
    /// </summary>
    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Xử lý yêu cầu GET đồng bộ để hiển thị trang.
    /// </summary>
    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        if (int.TryParse(HttpContext.Request.Query["code"], out var code))
        {
            StatusCode = code;
        }
    }
}
