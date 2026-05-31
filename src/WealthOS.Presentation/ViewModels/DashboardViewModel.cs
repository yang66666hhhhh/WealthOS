using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;

namespace WealthOS.Presentation.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly DashboardService _service;

    [ObservableProperty]
    private DashboardDto _dashboard = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _selectedTimeRange;

    public ISeries[] AssetSeries { get; set; } = [];
    public ISeries[] NetWorthSeries { get; set; } = [];
    public Axis[] NetWorthXAxes { get; set; } = [];
    public Axis[] NetWorthYAxes { get; set; } = [];

    public DashboardViewModel(DashboardService service)
    {
        _service = service;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ClearError();
        try
        {
            var days = SelectedTimeRange switch
            {
                0 => 30,
                1 => 90,
                2 => 365,
                3 => 3650,
                _ => 90
            };
            Dashboard = await _service.GetDashboardAsync(days);
            UpdateCharts();
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SetTimeRangeAsync(string range)
    {
        SelectedTimeRange = range switch
        {
            "30d" => 0,
            "90d" => 1,
            "1y" => 2,
            "all" => 3,
            _ => 1
        };
        await LoadDataAsync();
    }

    private void UpdateCharts()
    {
        var colors = new SKColor[]
        {
            SKColors.Teal,
            SKColors.Coral,
            SKColors.Gold,
            SKColors.MediumPurple,
            SKColors.SteelBlue,
            SKColors.Salmon
        };

        AssetSeries = Dashboard.AssetAllocations.Select((a, i) =>
            new PieSeries<decimal>
            {
                Values = [a.Value],
                Name = a.Name,
                Fill = new SolidColorPaint(colors[i % colors.Length]),
                InnerRadius = 60,
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                DataLabelsFormatter = p => $"{a.Name}: {a.Percentage:F1}%"
            }).ToArray();

        if (Dashboard.NetWorthHistory.Count > 0)
        {
            NetWorthSeries =
            [
                new LineSeries<decimal>
                {
                    Values = Dashboard.NetWorthHistory.Select(p => p.Value).ToArray(),
                    Fill = new SolidColorPaint(SKColors.Teal.WithAlpha(50)),
                    Stroke = new SolidColorPaint(SKColors.Teal) { StrokeThickness = 2 },
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.5
                }
            ];

            NetWorthXAxes =
            [
                new Axis
                {
                    Labels = Dashboard.NetWorthHistory.Select(p => p.Date.ToString("MM/dd")).ToArray(),
                    LabelsRotation = 0,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 },
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            ];

            NetWorthYAxes =
            [
                new Axis
                {
                    Labeler = value => value switch
                    {
                        >= 100_000_000 => $"{value / 100_000_000:N1}亿",
                        >= 10_000 => $"{value / 10_000:N1}万",
                        _ => $"{value:N0}"
                    },
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 },
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            ];
        }

        OnPropertyChanged(nameof(AssetSeries));
        OnPropertyChanged(nameof(NetWorthSeries));
        OnPropertyChanged(nameof(NetWorthXAxes));
        OnPropertyChanged(nameof(NetWorthYAxes));
    }
}
