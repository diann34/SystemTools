using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using SystemTools.Settings;

namespace SystemTools.Actions;

[ActionInfo("SystemTools.LoadTemporaryClassPlan", "加载临时课表", "\uE6A1", false)]
public class LoadTemporaryClassPlanAction(
    ILogger<LoadTemporaryClassPlanAction> logger,
    IProfileService profileService,
    IExactTimeService exactTimeService) : ActionBase<LoadTemporaryClassPlanSettings>
{
    private readonly ILogger<LoadTemporaryClassPlanAction> _logger = logger;
    private readonly IProfileService _profileService = profileService;
    private readonly IExactTimeService _exactTimeService = exactTimeService;

    protected override async Task OnInvoke()
    {
        if (!Guid.TryParse(Settings.ClassPlanId, out var classPlanId))
        {
            _logger.LogWarning("加载临时课表失败：课表ID无效。ClassPlanId={ClassPlanId}", Settings.ClassPlanId);
            return;
        }

        if (!_profileService.Profile.ClassPlans.TryGetValue(classPlanId, out var classPlan) || classPlan.IsOverlay)
        {
            _logger.LogWarning("加载临时课表失败：未找到课表或课表不可用。ClassPlanId={ClassPlanId}", classPlanId);
            return;
        }

        _profileService.Profile.TempClassPlanId = classPlanId;
        _profileService.Profile.TempClassPlanSetupTime = _exactTimeService.GetCurrentLocalDateTime();
        _profileService.SaveProfile();
        _logger.LogInformation("已加载临时课表：{ClassPlanName} ({ClassPlanId})", classPlan.Name, classPlanId);

        await base.OnInvoke();
    }
}
