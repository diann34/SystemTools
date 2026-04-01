using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using Avalonia.Threading;
using ClassIsland.Core.Controls;

namespace SystemTools.Views;

public partial class AdvancedShutdownDialog : MyWindow
{
    public AdvancedShutdownDialog()
    {
        InitializeComponent();
        
        this.GetPropertyChangedObservable(Window.WindowStateProperty).Subscribe(e =>
        {
            if (this.WindowState == WindowState.Minimized)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    this.WindowState = WindowState.Normal;
                    this.Activate();
                    this.InvalidateVisual();
                    
                    // var pos = this.Position;
                    // this.Position = pos.WithX(pos.X + 1);
                    // this.Position = pos;
                    
                }, DispatcherPriority.MaxValue);
            }
        });
    }

    public TextBlock? CountdownTextBlock => this.FindControl<TextBlock>("CountdownTextBlockElement");
    public ProgressBar? CountdownProgressBar => this.FindControl<ProgressBar>("CountdownProgressBarElement");
    public Button? ImmediateShutdownButton => this.FindControl<Button>("ImmediateShutdownButtonElement");
    public Button? ReadButton => this.FindControl<Button>("ReadButtonElement");
    public Button? CancelPlanButton => this.FindControl<Button>("CancelPlanButtonElement");
    public Button? ExtendButton => this.FindControl<Button>("ExtendButtonElement");

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
