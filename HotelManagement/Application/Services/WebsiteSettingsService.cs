using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Core.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HotelManagement.Application.Services;

public class WebsiteSettingsService : IWebsiteSettingsService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public WebsiteSettingsService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<WebsiteSettings> GetCurrentAsync()
    {
        await Task.CompletedTask;
        var settings = _configuration.GetSection("WebsiteSettings").Get<WebsiteSettings>();
        return settings ?? new WebsiteSettings();
    }

    public async Task<(bool Success, string Message)> UpdateAsync(WebsiteSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.LogoUrl))
            settings.LogoUrl = "/images/logo.svg";

        if (string.IsNullOrWhiteSpace(settings.FooterMapEmbedUrl))
            settings.FooterMapEmbedUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3928.8475510619946!2d105.7797746757655!3d10.029444372545898!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31a0882139720a77%3A0x6717aa348f057f0!2zMTIzIMSQxrDhu51uZyBIw7JhIELDrG5oLCBOaW5oIEtp4buBdSwgQ-G6p24gVGjGoSwgVmnhu4d0IE5hbQ!5e0!3m2!1svi!2s!4v1709708000000!5m2!1svi!2s";

        if (string.IsNullOrWhiteSpace(settings.BrandFullName))
            return (false, "Tên thương hiệu đầy đủ là bắt buộc.");

        if (string.IsNullOrWhiteSpace(settings.ContactPhone))
            return (false, "Số điện thoại liên hệ là bắt buộc.");

        if (string.IsNullOrWhiteSpace(settings.ContactEmail))
            return (false, "Email liên hệ là bắt buộc.");

        if (string.IsNullOrWhiteSpace(settings.ContactAddress))
            return (false, "Địa chỉ liên hệ là bắt buộc.");

        if (string.IsNullOrWhiteSpace(settings.FooterDescription))
            return (false, "Mô tả footer là bắt buộc.");

        var configPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
        await _writeLock.WaitAsync();
        try
        {
            JsonObject root;
            if (File.Exists(configPath))
            {
                var json = await File.ReadAllTextAsync(configPath);
                root = JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
            }
            else
            {
                root = new JsonObject();
            }

            root["WebsiteSettings"] = JsonSerializer.SerializeToNode(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var output = root.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(configPath, output);
            if (_configuration is IConfigurationRoot configRoot)
            {
                configRoot.Reload();
            }
        }
        finally
        {
            _writeLock.Release();
        }

        return (true, "Đã cập nhật cài đặt website thành công.");
    }
}
