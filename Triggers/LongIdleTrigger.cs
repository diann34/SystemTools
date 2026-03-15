using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using ClassIsland.Shared.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;

namespace SystemTools.Triggers;

[TriggerInfo("SystemTools.LongIdleTrigger", "长时间未操作电脑时", "\uE639")]
public partial class LongIdleTrigger(ILogger<LongIdleTrigger> logger) : TriggerBase<LongIdleTriggerConfig>
{
    private readonly ILogger<LongIdleTrigger> _logger = logger;
    private Timer? _timer;
    private int _hasTriggered;

    public override void Loaded()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    public override void UnLoaded()
    {
        if (_timer == null)
        {
            return;
        }

        _timer.Elapsed -= OnTimerElapsed;
        _timer.Stop();
        _timer.Dispose();
        _timer = null;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var idle = GetIdleTime();
            var threshold = TimeSpan.FromMinutes(Math.Max(1, Settings.IdleMinutes));

            if (idle >= threshold)
            {
                if (Interlocked.CompareExchange(ref _hasTriggered, 1, 0) != 0)
                {
                    return;
                }

                _logger.LogInformation("超过未操作阈值，触发自动化。Idle={Idle}, Threshold={Threshold}", idle, threshold);
                Trigger();
                return;
            }

            if (Interlocked.CompareExchange(ref _hasTriggered, 0, 1) == 0)
            {
                return;
            }

            if (AssociatedWorkflow?.ActionSet?.IsRevertEnabled == true &&
                AssociatedWorkflow.ActionSet.Status == ActionSetStatus.IsOn)
            {
                _logger.LogInformation("检测到用户恢复操作，触发恢复。Idle={Idle}", idle);
                TriggerRevert();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检测用户空闲状态时出错");
        }
    }

    private static TimeSpan GetIdleTime()
    {
        var info = new LASTINPUTINFO();
        info.cbSize = (uint)Marshal.SizeOf(info);

        if (!GetLastInputInfo(ref info))
        {
            return TimeSpan.Zero;
        }

        var lastInputTick = info.dwTime;
        var currentTick = (uint)Environment.TickCount;
        var elapsed = currentTick >= lastInputTick
            ? currentTick - lastInputTick
            : uint.MaxValue - lastInputTick + currentTick;

        return TimeSpan.FromMilliseconds(elapsed);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetLastInputInfo(ref LASTINPUTINFO plii);
}
