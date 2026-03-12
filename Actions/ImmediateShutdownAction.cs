using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SystemTools.Actions;

[ActionInfo("SystemTools.ImmediateShutdown", "立即关机", "\uEDE9", false)]
public class ImmediateShutdownAction(ILogger<ImmediateShutdownAction> logger) : ActionBase
{
    private readonly ILogger<ImmediateShutdownAction> _logger = logger;

    protected override async Task OnInvoke()
    {
        _logger.LogDebug("ImmediateShutdownAction OnInvoke 开始");

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "-s -t 0",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(psi);
            _logger.LogInformation("已执行立即关机命令");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行立即关机失败");
            throw;
        }

        await base.OnInvoke();
        _logger.LogDebug("ImmediateShutdownAction OnInvoke 完成");
    }
}
