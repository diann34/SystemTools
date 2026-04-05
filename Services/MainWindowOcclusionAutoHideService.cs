using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemTools.Shared;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace SystemTools.Services;

public class MainWindowOcclusionAutoHideService(ILogger<MainWindowOcclusionAutoHideService> logger)
{
    private readonly ILogger<MainWindowOcclusionAutoHideService> _logger = logger;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly SemaphoreSlim _ocrLock = new(1, 1);
    private readonly int _currentProcessId = Process.GetCurrentProcess().Id;
    private bool? _isHidden;
    private IntPtr _cachedMainWindowHandle = IntPtr.Zero;

    public void Start()
    {
        _timer.Tick -= OnTick;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void RefreshNow()
    {
        _ = CheckAndToggleAsync();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _ = CheckAndToggleAsync();
    }

    private async Task CheckAndToggleAsync()
    {
        if (GlobalConstants.MainConfig?.Data.AutoHideMainWindowWhenOccluded != true)
        {
            return;
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        if (!await _ocrLock.WaitAsync(0))
        {
            return;
        }

        try
        {
            var handle = ResolveMainWindowHandle();
            if (handle == IntPtr.Zero || !GetWindowRect(handle, out var rect))
            {
                return;
            }

            var captureRect = BuildCaptureRect(rect);
            if (captureRect.Width <= 1 || captureRect.Height <= 1)
            {
                return;
            }

            var textLength = await DetectTextLengthAsync(captureRect);
            var shouldHide = textLength > 4;

            if (_isHidden == shouldHide)
            {
                return;
            }

            ShowWindow(handle, shouldHide ? SW_HIDE : SW_SHOWNA);
            _isHidden = shouldHide;
            _logger.LogDebug("主界面遮挡检测: 文本长度={TextLength}, 动作={Action}", textLength, shouldHide ? "隐藏" : "显示");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "主界面遮挡检测失败，将在下次重试。");
        }
        finally
        {
            _ocrLock.Release();
        }
    }

    private IntPtr ResolveMainWindowHandle()
    {
        if (_cachedMainWindowHandle != IntPtr.Zero && IsWindow(_cachedMainWindowHandle))
        {
            return _cachedMainWindowHandle;
        }

        var processHandle = Process.GetCurrentProcess().MainWindowHandle;
        if (processHandle != IntPtr.Zero && IsWindow(processHandle))
        {
            _cachedMainWindowHandle = processHandle;
            return processHandle;
        }

        var discovered = FindWindowByProcessId(_currentProcessId);
        if (discovered != IntPtr.Zero)
        {
            _cachedMainWindowHandle = discovered;
        }

        return discovered;
    }

    private static IntPtr FindWindowByProcessId(int processId)
    {
        IntPtr found = IntPtr.Zero;
        EnumWindows((hWnd, _) =>
        {
            if (!IsWindow(hWnd))
            {
                return true;
            }

            _ = GetWindowThreadProcessId(hWnd, out var windowProcessId);
            if (windowProcessId == processId)
            {
                found = hWnd;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return found;
    }

    private static Rectangle BuildCaptureRect(RECT rect)
    {
        var screen = Screen.FromRectangle(Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom));
        var bounds = screen.Bounds;

        var windowWidth = Math.Max(1, rect.Right - rect.Left);
        var windowHeight = Math.Max(1, rect.Bottom - rect.Top);

        var width = Math.Clamp(windowWidth, 50, bounds.Width);
        var height = Math.Clamp(windowHeight / 2, 24, Math.Max(24, bounds.Height / 4));

        var x = Math.Clamp(rect.Left, bounds.Left, bounds.Right - width);
        var y = Math.Clamp(rect.Bottom + 4, bounds.Top, bounds.Bottom - height);

        return new Rectangle(x, y, width, height);
    }

    private static async Task<int> DetectTextLengthAsync(Rectangle captureRect)
    {
        using var bitmap = new Bitmap(captureRect.Width, captureRect.Height);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(captureRect.Left, captureRect.Top, 0, 0, captureRect.Size);
        }

        using var ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        ms.Position = 0;

        using var randomAccessStream = new InMemoryRandomAccessStream();
        await randomAccessStream.WriteAsync(ms.ToArray().AsBuffer());
        randomAccessStream.Seek(0);

        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
        var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

        var engine = OcrEngine.TryCreateFromUserProfileLanguages();
        if (engine == null)
        {
            return 0;
        }

        var ocrResult = await engine.RecognizeAsync(softwareBitmap);
        var text = (ocrResult.Text ?? string.Empty).Replace("\r", "").Replace("\n", "").Trim();
        return text.Length;
    }

    private const int SW_HIDE = 0;
    private const int SW_SHOWNA = 8;

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr hWnd);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
