using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Data.Save;
using CoreSystem.Player;
using CoreSystem.UI;

namespace CoreSystem.UI.SystemMenu
{
    public sealed class SystemMenuWindow
    {
        #region [Variables/Fields]
        public const string ModalName = "System Menu";
        public const string Title = "System";
        public const int QuickSaveSlot = 0;

        private readonly SettingsWindow _settings = new SettingsWindow();
        private readonly SystemConfirmDialog _confirm = new SystemConfirmDialog();
        private UIGlobalModal _modal;

        public event Action<string> OnItemSelected;
        #endregion

        #region [Properties]
        public bool IsOpen => _modal != null && _modal.IsOpen;
        public SettingsWindow Settings => _settings;
        public SystemConfirmDialog Confirm => _confirm;

        public IReadOnlyList<SystemMenuItem> Items => new List<SystemMenuItem>
        {
            new SystemMenuItem("Character", () => OpenWindow("CharacterStatus")),
            new SystemMenuItem("Equipment", () => OpenWindow("CharacterEquipment")),
            new SystemMenuItem("Inventory", () => OpenWindow("Inventory")),
            new SystemMenuItem("Skills", () => OpenWindow("CharacterSkillManage")),
            new SystemMenuItem("Passive Tree", () => OpenWindow("CharacterSkillTree")),
            SystemMenuItem.Separator,
            new SystemMenuItem("Keybind", OpenKeybinds),
            new SystemMenuItem("Settings", OpenSettings),
            SystemMenuItem.Separator,
            new SystemMenuItem("Save Game", SaveGame, PlayerCore.Instance != null && PlayerCore.GetPlayer() != null),
            new SystemMenuItem("Load Game", AskLoadGame, PlayerCore.Instance != null && SaveStorage.SlotExists(QuickSaveSlot)),
            SystemMenuItem.Separator,
            new SystemMenuItem("Exit Game", AskExitGame)
        };
        #endregion

        #region [Public Methods]
        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            _modal = UIGlobalModal.GetOrCreate(ModalName);
            Setup(_modal);
        }

        public void Setup(UIGlobalModal modal)
        {
            if (modal == null) return;
            UIMiniHeaderStyleRegistry.Prepare(modal, Title)
                .SetScrollable(false)
                .SetWindowSize(new Vector2(UIMiniHeaderStyleRegistry.SystemMenuWidth, 420f))
                .Center()
                .Build(BuildUI)
                .FitContent(true, null, false);
        }

        public void Close()
        {
            if (_modal != null && _modal.IsOpen) _modal.CloseSelf();
        }
        #endregion

        #region [Private Methods]
        private void BuildUI(UIBuilderContext ui)
        {
            ui.Column(list =>
            {
                foreach (SystemMenuItem item in Items)
                {
                    if (item.IsSeparator)
                    {
                        list.Divider(UIMiniHeaderStyleRegistry.Separator, 1f, UIMiniHeaderStyleRegistry.SeparatorColor).Fill();
                        continue;
                    }
                    SystemMenuItem captured = item;
                    list.Button(UIMiniHeaderStyleRegistry.MenuRow, item.Label, () => Select(captured))
                        .Interactable(item.Enabled)
                        .TextColor(item.Enabled ? UIMiniHeaderStyleRegistry.Text : UIMiniHeaderStyleRegistry.Disabled)
                        .TextAlign(TMPro.TextAlignmentOptions.MidlineLeft)
                        .Fill();
                }
            }).Gap(1f).Fill();
        }

        private void Select(SystemMenuItem item)
        {
            if (!item.Enabled || item.Action == null) return;
            OnItemSelected?.Invoke(item.Label);
            item.Action.Invoke();
        }

        private void OpenWindow(string windowName)
        {
            Close();
            if (UIManager.Instance != null) UIManager.Instance.Open(windowName);
        }

        private void OpenSettings()
        {
            Close();
            _settings.Open();
        }

        private void OpenKeybinds()
        {
            Close();
            _settings.Open(CoreSystem.Settings.SettingKeys.PageControls);
        }

        private void SaveGame()
        {
            var core = PlayerCore.Instance;
            if (core == null || PlayerCore.GetPlayer() == null) return;
            bool saved = SaveStorage.SaveSlot(QuickSaveSlot, core.SaveData());
            Debug.Log(saved ? $"[SystemMenu] Saved to slot {QuickSaveSlot}." : "[SystemMenu] Save failed.");
            Close();
        }

        private void AskLoadGame()
        {
            Close();
            _confirm.Show("Load Game", "Load the last save? Unsaved progress will be lost.", "Load", LoadGame);
        }

        private void LoadGame()
        {
            var core = PlayerCore.Instance;
            var data = SaveStorage.LoadSlot(QuickSaveSlot);
            if (core == null || data == null) return;
            core.LoadData(data);
        }

        private void AskExitGame()
        {
            Close();
            _confirm.Show("Exit Game", "Return to the desktop? Unsaved progress will be lost.", "Exit", ExitGame);
        }

        private static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
    }
}
