using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ChartJSCore.Models;
using ChartJSCore.Helpers;

namespace HotelManagement.Areas.Admin.Pages.Dashboard
{
    [Authorize(Roles = "Manager,Receptionist")]
    /// <summary>
    /// PageModel xử lý trang quản trị 'Index.cshtml'.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        /// <summary>
        /// Khởi tạo lớp IndexModel và nạp các dependency cần thiết.
        /// </summary>
        public IndexModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public DashboardViewModel Data { get; private set; } = null!;
        public Chart RevenueChart { get; set; } = null!;
        public Chart RoomTypeChart { get; set; } = null!;

        /// <summary>
        /// Xử lý yêu cầu GET để nạp dữ liệu và hiển thị trang.
        /// </summary>
        public async Task OnGetAsync()
        {
            Data = await _dashboardService.GetDashboardDataAsync();
            InitializeCharts();
        }

        private void InitializeCharts()
        {
            // 1. Revenue Line Chart
            RevenueChart = new Chart
            {
                Type = Enums.ChartType.Line,
                Data = new Data
                {
                    Labels = new List<string> { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" },
                    Datasets = new List<Dataset>
                    {
                        new LineDataset
                        {
                            Label = "Doanh thu (VNĐ)",
                            Data = Data.MonthlyRevenue.Select(v => (double?)v).ToList(),
                            BorderColor = new List<ChartColor> { ChartColor.FromHexString("#6366f1") },
                            BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(99, 102, 241, 0.1) },
                            Fill = "origin",
                            Tension = 0.4,
                            PointRadius = new List<int> { 0 },
                            PointHoverRadius = new List<int> { 6 }
                        }
                    }
                },
                Options = new Options
                {
                    Responsive = true,
                    MaintainAspectRatio = false,
                    Plugins = new Plugins
                    {
                        Legend = new Legend { Display = false }
                    },
                    Scales = new Dictionary<string, Scale>
                    {
                        { "x", new CartesianLinearScale 
                            { 
                                Ticks = new Tick { Color = ChartColor.FromHexString("#94a3b8") }, 
                                Grid = new Grid { Display = false } 
                            } 
                        },
                        { "y", new CartesianLinearScale 
                            { 
                                Ticks = new Tick { Color = ChartColor.FromHexString("#94a3b8") }, 
                                Grid = new Grid { Color = new List<ChartColor> { ChartColor.FromRgba(255, 255, 255, 0.05) } } 
                            } 
                        }
                    }
                }
            };

            // 2. Room Type Doughnut Chart
            RoomTypeChart = new Chart
            {
                Type = Enums.ChartType.Doughnut,
                Data = new Data
                {
                    Labels = Data.RoomTypeDistribution.Keys.ToList(),
                    Datasets = new List<Dataset>
                    {
                        new PieDataset
                        {
                            Data = Data.RoomTypeDistribution.Values.Select(v => (double?)v).ToList(),
                            BackgroundColor = new List<ChartColor>
                            {
                                ChartColor.FromHexString("#6366f1"), // Indigo
                                ChartColor.FromHexString("#ec4899"), // Pink
                                ChartColor.FromHexString("#10b981"), // Emerald
                                ChartColor.FromHexString("#f59e0b")  // Amber
                            },
                            BorderWidth = new List<int> { 0 },
                            HoverOffset = 15
                        }
                    }
                },
                Options = new PieOptions
                {
                    Responsive = true,
                    MaintainAspectRatio = false,
                    Cutout = "75%",
                    Plugins = new Plugins
                    {
                        Legend = new Legend 
                        { 
                            Position = "bottom", 
                            Labels = new LegendLabel { Color = ChartColor.FromHexString("#94a3b8"), UsePointStyle = true } 
                        }
                    }
                }
            };
        }
    }
}
