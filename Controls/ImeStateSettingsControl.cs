using Avalonia.Controls;
using Avalonia.Layout;
using ClassIsland.Core.Abstractions.Controls;
using SystemTools.Settings;

namespace SystemTools.Controls;

public class ImeStateSettingsControl : ActionSettingsControlBase<ImeStateSettings>
{
    private readonly ToggleSwitch _toggleSwitch;

    public ImeStateSettingsControl()
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical};
        _toggleSwitch = new ToggleSwitch { Content = "启用输入法(IME)"};
        _toggleSwitch.IsCheckedChanged += (s, e) => Settings.EnableIme = _toggleSwitch.IsChecked ?? false;
        panel.Children.Add(_toggleSwitch);
        Content = panel;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _toggleSwitch.IsChecked = Settings.EnableIme;
    }
}
