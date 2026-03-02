using Avalonia.Controls;
using ClassIsland.Core.Abstractions.Controls;
using SystemTools.Settings;

namespace SystemTools.Controls;

public class AdvancedShutdownSettingsControl : ActionSettingsControlBase<AdvancedShutdownSettings>
{
    private NumericUpDown _minutesInput;

    public AdvancedShutdownSettingsControl()
    {
        var panel = new StackPanel { Spacing = 10, Margin = new(10) };

        panel.Children.Add(new TextBlock
        {
            Text = "高级计时关机设置",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 14
        });

        var minutesPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 5
        };

        minutesPanel.Children.Add(new TextBlock
        {
            Text = "初始倒计时（分钟）:",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });

        _minutesInput = new NumericUpDown
        {
            Width = 120,
            Minimum = 1,
            Maximum = 1440,
            Increment = 1
        };
        _minutesInput.ValueChanged += (_, _) => { Settings.Minutes = (int)(_minutesInput.Value ?? 2); };

        minutesPanel.Children.Add(_minutesInput);
        panel.Children.Add(minutesPanel);

        panel.Children.Add(new TextBlock
        {
            Text = "触发后会弹出独立对话框，可已阅、取消计划或延长时间。",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Foreground = Avalonia.Media.Brushes.Gray
        });

        Content = panel;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _minutesInput.Value = Settings.Minutes;
    }
}
