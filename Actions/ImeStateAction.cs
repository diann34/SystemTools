using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;
using SystemTools.Settings;
using Windows.Win32;

namespace SystemTools.Actions;

[ActionInfo("SystemTools.ImeState", "更改输入法状态", "\uE775", false)]
public class ImeStateAction(ILogger<ImeStateAction> logger) : ActionBase<ImeStateSettings>
{
    private readonly ILogger<ImeStateAction> _logger = logger;

    protected override async Task OnInvoke()
    {
        _logger.LogDebug("ImeStateAction OnInvoke 开始");
        var hwnd = PInvoke.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
        {
            _logger.LogError("ImeStateAction OnInvoke失败:未找到活动窗口");
            return;
        }
        var hIMC = PInvoke.ImmGetContext(hwnd);
        if (hIMC == IntPtr.Zero)
        {
            _logger.LogError("ImeStateAction OnInvoke失败:未获取到输入法上下文");
            return;
        }
        if (PInvoke.ImmSetOpenStatus(hIMC, Settings.EnableIme) == 0)
        {
            _logger.LogError(new Win32Exception(Marshal.GetLastWin32Error(), "ImmSetOpenStatus failed."), "更改输入法状态失败");
            return;
        }
        _logger.LogInformation("输入法状态已设置为: {State}", Settings.EnableIme ? "开启" : "关闭");
        await base.OnInvoke();
        _logger.LogDebug("ImeStateAction OnInvoke 完成");
    }
}
