using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SystemTools.Actions;

[ActionInfo("SystemTools.Sleep", "睡眠", "\uF44B", false)]
public class SleepAction(ILogger<SleepAction> logger) : ActionBase
{
    private readonly ILogger<SleepAction> _logger = logger;

    protected override async Task OnInvoke()
    {
        _logger.LogDebug("SleepAction OnInvoke 开始");

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "rundll32.exe",
                Arguments = "powrprof.dll,SetSuspendState 0,1,0",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(psi);
            _logger.LogInformation("已执行睡眠命令");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行睡眠失败");
            throw;
        }

        await base.OnInvoke();
        _logger.LogDebug("SleepAction OnInvoke 完成");
    }
}
