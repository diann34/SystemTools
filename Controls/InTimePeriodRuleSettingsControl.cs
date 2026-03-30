using Avalonia.Controls;
using Avalonia.Layout;
using ClassIsland.Core.Abstractions.Controls;
using System;
using SystemTools.Rules;

namespace SystemTools.Controls;

public class InTimePeriodRuleSettingsControl : RuleSettingsControlBase<InTimePeriodRuleSettings>
{
    private readonly TextBox _startTextBox;
    private readonly TextBox _endTextBox;

    public InTimePeriodRuleSettingsControl()
    {
        var panel = new StackPanel { Spacing = 10, Margin = new(10) };
        panel.Children.Add(new TextBlock
        {
            Text = "设置时间段（24 小时制，格式 HH:mm 或 HH:mm:ss）：",
            FontWeight = Avalonia.Media.FontWeight.Bold
        });

        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,*,Auto"),
            ColumnSpacing = 8,
            VerticalAlignment = VerticalAlignment.Center
        };

        row.Children.Add(new TextBlock { Text = "起始", VerticalAlignment = VerticalAlignment.Center });
        _startTextBox = new TextBox { Watermark = "08:00", HorizontalAlignment = HorizontalAlignment.Stretch };
        Grid.SetColumn(_startTextBox, 1);
        row.Children.Add(_startTextBox);

        var sep = new TextBlock { Text = "至", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(sep, 2);
        row.Children.Add(sep);

        _endTextBox = new TextBox { Watermark = "18:00", HorizontalAlignment = HorizontalAlignment.Stretch };
        Grid.SetColumn(_endTextBox, 3);
        row.Children.Add(_endTextBox);

        var hint = new TextBlock
        {
            Text = "若起始晚于结束，将按跨天时间段处理（例如 22:00 - 06:00）。",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Foreground = Avalonia.Media.Brushes.Gray
        };

        panel.Children.Add(row);
        panel.Children.Add(hint);
        Content = panel;

        _startTextBox.LostFocus += (_, _) => ApplyInput(_startTextBox, true);
        _endTextBox.LostFocus += (_, _) => ApplyInput(_endTextBox, false);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _startTextBox.Text = Settings.StartTime;
        _endTextBox.Text = Settings.EndTime;
    }

    private void ApplyInput(TextBox box, bool isStart)
    {
        var text = box.Text?.Trim();
        if (!TimeSpan.TryParse(text, out var parsed))
        {
            box.Text = isStart ? Settings.StartTime : Settings.EndTime;
            return;
        }

        var normalized = parsed.ToString(@"hh\:mm\:ss");
        box.Text = normalized;
        if (isStart)
        {
            Settings.StartTime = normalized;
        }
        else
        {
            Settings.EndTime = normalized;
        }
    }
}
