using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Controls.Primitives;
using ClassIsland.Core.Abstractions.Controls;
using SystemTools.Settings;

namespace SystemTools.Controls;

public class ShowFloatingWindowSettingsControl : ActionSettingsControlBase<ShowFloatingWindowSettings>
{
    private CheckBox _toggleCheckBox;

    public ShowFloatingWindowSettingsControl()
    {
        var panel = new StackPanel { Spacing = 10, Margin = new(10) };

        panel.Children.Add(new TextBlock
        {
            Text = "悬浮窗设置",
            FontSize = 14
        });

        _toggleCheckBox = new CheckBox
        {
            Content = "开启悬浮窗（取消勾选则关闭）",
            IsChecked = true
        };

        panel.Children.Add(_toggleCheckBox);

        Content = panel;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _toggleCheckBox[!ToggleButton.IsCheckedProperty] = new Binding(nameof(Settings.ShowFloatingWindow))
        {
            Source = Settings,
            Mode = BindingMode.TwoWay
        };
    }
}
