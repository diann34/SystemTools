using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using SystemTools.Settings;

namespace SystemTools.Controls;

public class BackgroundPlayAudioSettingsControl : ActionSettingsControlBase<BackgroundPlayAudioSettings>
{
    private readonly TextBox _audioPathBox;
    private readonly CheckBox _waitForCompletedCheckBox;

    public BackgroundPlayAudioSettingsControl()
    {
        var panel = new StackPanel { Spacing = 10, Margin = new(10) };

        panel.Children.Add(new TextBlock
        {
            Text = "后台播放音频",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 14
        });

        var pathPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 10
        };

        _audioPathBox = new TextBox
        {
            Watermark = "点击“浏览...”选择音频文件",
            Width = 320,
            IsReadOnly = true
        };
        pathPanel.Children.Add(_audioPathBox);

        var browseButton = new Button
        {
            Content = "浏览...",
            Width = 80
        };
        browseButton.Click += async (_, _) => await BrowseAudioFileAsync();
        pathPanel.Children.Add(browseButton);

        panel.Children.Add(pathPanel);

        _waitForCompletedCheckBox = new CheckBox
        {
            Content = "播放后等待播放完成",
            IsChecked = false
        };
        _waitForCompletedCheckBox.IsCheckedChanged += (_, _) =>
        {
            Settings.WaitForPlaybackCompleted = _waitForCompletedCheckBox.IsChecked == true;
        };
        panel.Children.Add(_waitForCompletedCheckBox);

        Content = panel;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _audioPathBox.Text = Settings.AudioFilePath;
        _waitForCompletedCheckBox.IsChecked = Settings.WaitForPlaybackCompleted;
    }

    private async Task BrowseAudioFileAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
            {
                var logger = IAppHost.TryGetService<ILogger<BackgroundPlayAudioSettingsControl>>();
                logger?.LogWarning("无法获取 TopLevel");
                return;
            }

            var options = new FilePickerOpenOptions
            {
                Title = "选择音频文件",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("音频文件") { Patterns = ["*.mp3", "*.wav", "*.flac", "*.ogg", "*.m4a", "*.aac"] },
                    new FilePickerFileType("所有文件") { Patterns = ["*"] }
                ]
            };

            var result = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            if (result != null && result.Count > 0)
            {
                Settings.AudioFilePath = result[0].Path.LocalPath;
                _audioPathBox.Text = Settings.AudioFilePath;
            }
        }
        catch (Exception ex)
        {
            var logger = IAppHost.TryGetService<ILogger<BackgroundPlayAudioSettingsControl>>();
            logger?.LogError(ex, "选择音频文件失败");
        }
    }
}
