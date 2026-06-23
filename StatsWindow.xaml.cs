using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Pomodoro.Models;
using Pomodoro.Services;

namespace Pomodoro
{
    /// <summary>Read-only stats dialog: streak, totals, and a day×hour focus heatmap over the session log.</summary>
    public partial class StatsWindow : Window
    {
        private const int HoursPerDay = 24;
        private const double CellSize = 15.0;
        private const double CellGap = 2.0;
        private const double LabelColumnWidth = 34.0;

        // Monday-first week: the value is the DayOfWeek int (Sunday = 0) each row maps to.
        private static readonly (string Label, int DayOfWeekIndex)[] WeekRows =
        {
            ("Mon", 1), ("Tue", 2), ("Wed", 3), ("Thu", 4), ("Fri", 5), ("Sat", 6), ("Sun", 0)
        };

        private static readonly Color HeatColor = Color.FromRgb(0x4C, 0xAF, 0x50);

        public StatsWindow(ISessionLog sessionLog)
        {
            InitializeComponent();

            IReadOnlyList<CompletedPomodoro> entries = sessionLog.All();
            RenderSummary(entries);
            RenderHeatmap(SessionStats.WeeklyHeatmap(entries));
        }

        private void RenderSummary(IReadOnlyList<CompletedPomodoro> entries)
        {
            DateTime today = DateTime.Now;
            int streak = SessionStats.CurrentStreak(entries, today);
            int total = entries.Count;
            int todayCount = entries.Count(entry => entry.CompletedAt.Date == today.Date);

            SummaryText.Text = $"🔥 Streak: {streak}    •    Today: {todayCount}    •    Total: {total} pomodoros";
        }

        private void RenderHeatmap(int[,] grid)
        {
            int peak = Peak(grid);

            HeatmapGrid.RowDefinitions.Clear();
            HeatmapGrid.ColumnDefinitions.Clear();
            HeatmapGrid.Children.Clear();

            BuildColumns();
            BuildHourHeaderRow();
            BuildDayRows(grid, peak);
        }

        private void BuildColumns()
        {
            HeatmapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(LabelColumnWidth) });
            for (int hour = 0; hour < HoursPerDay; hour++)
            {
                HeatmapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize + CellGap) });
            }
        }

        private void BuildHourHeaderRow()
        {
            HeatmapGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int hour = 0; hour < HoursPerDay; hour += 3)
            {
                TextBlock label = new TextBlock
                {
                    Text = hour.ToString("00"),
                    Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                    FontSize = 9,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, hour + 1);
                HeatmapGrid.Children.Add(label);
            }
        }

        private void BuildDayRows(int[,] grid, int peak)
        {
            for (int row = 0; row < WeekRows.Length; row++)
            {
                HeatmapGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                int gridRow = row + 1;

                AddDayLabel(WeekRows[row].Label, gridRow);

                int dayIndex = WeekRows[row].DayOfWeekIndex;
                for (int hour = 0; hour < HoursPerDay; hour++)
                {
                    int count = grid[dayIndex, hour];
                    HeatmapGrid.Children.Add(BuildCell(count, peak, gridRow, hour + 1));
                }
            }
        }

        private void AddDayLabel(string text, int gridRow)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(label, gridRow);
            Grid.SetColumn(label, 0);
            HeatmapGrid.Children.Add(label);
        }

        private Border BuildCell(int count, int peak, int gridRow, int gridColumn)
        {
            Border cell = new Border
            {
                Width = CellSize,
                Height = CellSize,
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, CellGap, CellGap, 0),
                Background = new SolidColorBrush(CellColor(count, peak)),
                ToolTip = count == 0 ? null : $"{count} pomodoros"
            };
            Grid.SetRow(cell, gridRow);
            Grid.SetColumn(cell, gridColumn);
            return cell;
        }

        private static Color CellColor(int count, int peak)
        {
            if (count == 0 || peak == 0)
            {
                return Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF);
            }

            double intensity = (double)count / peak;
            byte alpha = (byte)(0x33 + intensity * (0xFF - 0x33));
            return Color.FromArgb(alpha, HeatColor.R, HeatColor.G, HeatColor.B);
        }

        private static int Peak(int[,] grid)
        {
            int peak = 0;
            foreach (int count in grid)
            {
                if (count > peak)
                {
                    peak = count;
                }
            }

            return peak;
        }
    }
}
