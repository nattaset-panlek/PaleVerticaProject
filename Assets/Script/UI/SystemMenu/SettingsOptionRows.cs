using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using CoreSystem.Settings;
using CoreSystem.Settings.Options;
using CoreSystem.UI;
using CoreSystem.UI.Utility;

namespace CoreSystem.UI.SystemMenu
{
    public static class SettingsOptionRows
    {
        #region [Variables/Fields]
        public const float ValueWidth = 48f;
        public const float InlineWidth = 44f;
        #endregion

        #region [Public Methods]
        public static void BuildPage(UIBuilderContext ui, SettingsWindow window, SettingsPage page)
        {
            List<SettingOption> options = SettingsRegistry.GetOptions(page.Id).Where(o => !o.IsInline).ToList();
            if (options.Count == 0)
            {
                ui.Label("Nothing to set here yet.").TextColor(UIMiniHeaderStyleRegistry.Sub).Fill();
                return;
            }

            if (SettingsRegistry.GetOptions(page.Id).Any(o => o.IsInline)) InlineHeader(ui, SettingsRegistry.GetOptions(page.Id).First(o => o.IsInline).Label);

            foreach (string group in SettingsRegistry.GetGroups(page.Id))
            {
                var inGroup = options.Where(o => (o.Group ?? string.Empty) == group).ToList();
                if (inGroup.Count == 0) continue;
                if (string.IsNullOrEmpty(group))
                {
                    foreach (var option in inGroup) Row(ui, window, option);
                    continue;
                }

                window.GroupHeader(ui, page.Id, group);
                if (!window.IsExpanded(page.Id, group)) continue;
                ui.Column(body =>
                {
                    foreach (var option in inGroup) Row(body, window, option);
                }).Padding(14, 0, 0, 4).Gap(2f).Fill();
            }
        }

        public static void Row(UIBuilderContext ui, SettingsWindow window, SettingOption option)
        {
            ui.Row(row =>
            {
                var label = row.Label(option.Label.ToUpperInvariant()).TextColor(UIMiniHeaderStyleRegistry.Text).Fill();
                if (!string.IsNullOrEmpty(option.Tooltip) && label.GameObject != null)
                {
                    var text = label.TextComponent;
                    if (text != null) text.raycastTarget = true;
                    var hover = label.GameObject.GetComponent<HoverableUI>() ?? label.GameObject.AddComponent<HoverableUI>();
                    hover.Setup(option.Label, option.Tooltip);
                }
                Control(row, window, option);
                foreach (var inline in SettingsRegistry.GetInline(option.Key)) InlineControl(row, window, inline);
            }).Height(UIMiniHeaderStyleRegistry.SettingsRowHeight).Align(TextAnchor.MiddleLeft).Fill();
        }
        #endregion

        #region [Private Methods]
        private static void Control(UIBuilderContext row, SettingsWindow window, SettingOption option)
        {
            var draft = window.Draft;
            switch (option)
            {
                case SliderSettingOption slider:
                    TMP_Text value = null;
                    float current = (float)slider.Normalize(draft.Get(option.Key));
                    row.Slider(UIMiniHeaderStyleRegistry.Slider, current, slider.Min, slider.Max, slider.WholeNumbers, v =>
                    {
                        draft.Set(option.Key, v);
                        if (value != null) value.text = slider.Format(v);
                        window.MarkChanged();
                    }).Width(UIMiniHeaderStyleRegistry.SettingsControlWidth);
                    value = row.Label(slider.Format(current)).TextColor(UIMiniHeaderStyleRegistry.Text).TextAlign(TextAlignmentOptions.MidlineRight).Width(ValueWidth).TextComponent;
                    break;
                case ChoiceSettingOption choice:
                    row.Dropdown(UIMiniHeaderStyleRegistry.Dropdown, choice.Labels, (int)choice.Normalize(draft.Get(option.Key)), i =>
                    {
                        draft.Set(option.Key, i);
                        window.MarkChanged();
                    }).Width(UIMiniHeaderStyleRegistry.SettingsControlWidth);
                    break;
                case ToggleSettingOption toggle:
                    string style = toggle.Kind == SettingOptionKind.Checkbox ? UIMiniHeaderStyleRegistry.Checkbox : UIMiniHeaderStyleRegistry.Switch;
                    row.Toggle(style, draft.Get<bool>(option.Key), string.Empty, v =>
                    {
                        draft.Set(option.Key, v);
                        window.MarkChanged();
                    });
                    break;
            }
        }

        private static void InlineControl(UIBuilderContext row, SettingsWindow window, SettingOption option)
        {
            row.Row(cell =>
            {
                Control(cell, window, option);
            }).Width(InlineWidth).Align(TextAnchor.MiddleCenter);
        }

        private static void InlineHeader(UIBuilderContext ui, string label)
        {
            ui.Row(row =>
            {
                row.Spacer().Fill();
                row.Label(label.ToUpperInvariant()).FontSize(11f).TextColor(UIMiniHeaderStyleRegistry.Sub).TextAlign(TextAlignmentOptions.Center).Width(InlineWidth);
            }).Height(16f).Fill();
        }
        #endregion
    }
}
