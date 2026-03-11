using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using ClassIsland.Shared.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using SystemTools.Services;

namespace SystemTools.Triggers;

[TriggerInfo("SystemTools.FloatingWindowTrigger", "从悬浮窗触发", "\uEA37")]
public class FloatingWindowTrigger : TriggerBase<FloatingWindowTriggerConfig>
{
    private readonly FloatingWindowService _floatingWindowService;
    private readonly ILogger<FloatingWindowTrigger> _logger;

    public FloatingWindowTrigger(FloatingWindowService floatingWindowService, ILogger<FloatingWindowTrigger> logger)
    {
        _floatingWindowService = floatingWindowService;
        _logger = logger;
    }

    public override void Loaded()
    {
        Settings.PropertyChanged += OnSettingsChanged;
        if (AssociatedWorkflow?.ActionSet != null)
        {
            AssociatedWorkflow.ActionSet.PropertyChanged += OnActionSetPropertyChanged;
        }
        EnsureButtonId();
        _floatingWindowService.RegisterTrigger(this);
    }

    public override void UnLoaded()
    {
        Settings.PropertyChanged -= OnSettingsChanged;
        if (AssociatedWorkflow?.ActionSet != null)
        {
            AssociatedWorkflow.ActionSet.PropertyChanged -= OnActionSetPropertyChanged;
        }
        _floatingWindowService.UnregisterTrigger(this);
    }

    public void TriggerFromFloatingWindow()
    {
        var actionSet = AssociatedWorkflow?.ActionSet;
        var useRevertMode = actionSet?.IsRevertEnabled == true;
        var isOn = actionSet?.Status == ActionSetStatus.IsOn;

        _logger.LogInformation("从悬浮窗触发触发器: {ButtonId}, 启用恢复: {UseRevertMode}, 当前状态: {Status}",
            Settings.ButtonId, useRevertMode, actionSet?.Status);

        if (useRevertMode && isOn)
        {
            TriggerRevert();
            return;
        }

        Trigger();
    }

    public string GetButtonId()
    {
        EnsureButtonId();
        return Settings.ButtonId;
    }

    public string GetIcon()
    {
        return Settings.Icon;
    }

    public string GetButtonName()
    {
        return ShouldShowRevertButton() ? "恢复" : Settings.ButtonName;
    }

    public bool ShouldUseRevertStyle()
    {
        return ShouldShowRevertButton();
    }

    private void EnsureButtonId()
    {
        if (string.IsNullOrWhiteSpace(Settings.ButtonId))
        {
            Settings.ButtonId = Guid.NewGuid().ToString("N");
        }
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FloatingWindowTriggerConfig.Icon) ||
            e.PropertyName == nameof(FloatingWindowTriggerConfig.ButtonId) ||
            e.PropertyName == nameof(FloatingWindowTriggerConfig.ButtonName))
        {
            _floatingWindowService.RegisterTrigger(this);
        }
    }

    private void OnActionSetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Status" ||
            e.PropertyName == "IsRevertEnabled")
        {
            _floatingWindowService.RegisterTrigger(this);
        }
    }

    private bool ShouldShowRevertButton()
    {
        var actionSet = AssociatedWorkflow?.ActionSet;
        return actionSet?.IsRevertEnabled == true && actionSet.Status == ActionSetStatus.IsOn;
    }
}
