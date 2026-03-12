using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SystemTools.Actions;

[ActionInfo("SystemTools.ImmediateRestart", "立即重启", "\uE0BD", false)]
public class ImmediateRestartAction(ILogger<ImmediateRestartAction> logger) : ActionBase
{
    private readonly ILogger<ImmediateRestartAction> _logger = logger;

    protected override async Task OnInvoke()
    {
        _logger.LogDebug("ImmediateRestartAction OnInvoke 开始");

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "-r -t 0",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(psi);
            _logger.LogInformation("已执行立即重启命令");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行立即重启失败");
            throw;
        }

        await base.OnInvoke();
        _logger.LogDebug("ImmediateRestartAction OnInvoke 完成");
    }
}
