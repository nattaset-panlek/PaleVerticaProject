using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using CoreSystem.Settings;
using CoreSystem.UI;

namespace CoreSystem.UI.SystemMenu
{
    public sealed class SettingsWindow
    {
        #region [Variables/Fields]
        public const string ModalName = "Settings";
        public const string BodyRegion = "settings.body";

        public static readonly HashSet<string> CollapsedByDefault = new HashSet<string>
        {
            SettingKeys.GroupLanguage,
            SettingKeys.PageControls + "/Combat",
            SettingKeys.PageControls + "/Actions",
            SettingKeys.PageControls + "/Hotbar",
            SettingKeys.PageControls + "/Interface"
        };

        private UIGlobalModal _modal;
        private int _tab;
        private SettingsDraft _applied;
        private SettingsDraft _draft;
        private UIElementRef _applyButton;
        private TMP_Text _status;
        private string _notice;
        private readonly Dictionary<string, bool> _expanded = new Dictionary<string, bool>();

        public event Action OnApplied;
        public event Action OnClosed;
        #endregion

        #region [Properties]
        public bool IsOpen => _modal != null && _modal.IsOpen;
        public int Tab => _tab;
        public SettingsDraft Draft => _draft;
        public UIGlobalModal Modal => _modal;
        public IReadOnlyList<SettingsPage> Pages => SettingsRegistry.Pages;
        public SettingsPage CurrentPage => Pages.Count > 0 ? Pages[Mathf.Clamp(_tab, 0, Pages.Count - 1)] : null;
        public bool HasChanges => _draft != null && _draft.Differences(_applied).Count > 0;
        public List<string> PendingChanges => _draft != null ? _draft.Differences(_applied) : new List<string>();
        #endregion

        #region [Public Methods]
        public void Open(int tab = 0)
        {
            _tab = tab;
            _modal = UIGlobalModal.GetOrCreate(ModalName);
            Setup(_modal);
        }

        public void Open(string pageId)
        {
            Open(Mathf.Max(0, IndexOf(pageId)));
        }

        public void Setup(UIGlobalModal modal, int tab)
        {
            _tab = tab;
            Setup(modal);
        }

        public void Setup(UIGlobalModal modal)
        {
            if (modal == null) return;
            if (_modal != null) _modal.OnClosed -= HandleClosed;
            _modal = modal;
            _modal.OnClosed += HandleClosed;
            _tab = Mathf.Clamp(_tab, 0, Mathf.Max(0, Pages.Count - 1));
            _applied = SettingsDraft.Capture();
            _draft = _applied.Clone();
            _notice = null;
            UIMiniHeaderStyleRegistry.Prepare(modal, ModalName)
                .SetScrollable(false)
                .SetWindowSize(UIMiniHeaderStyleRegistry.SettingsWindowSize)
                .Center()
                .Build(BuildUI);
        }

        public void ShowTab(int tab)
        {
            _tab = Mathf.Clamp(tab, 0, Mathf.Max(0, Pages.Count - 1));
            _notice = null;
            Rebuild();
        }

        public void ResetCurrentPage()
        {
            if (_draft == null || CurrentPage == null) return;
            _draft.ResetPage(CurrentPage.Id);
            _notice = null;
            Rebuild();
        }

        public void Apply()
        {
            if (_draft == null) return;
            _draft.Apply();
            _applied = SettingsDraft.Capture();
            _draft = _applied.Clone();
            _notice = "Settings applied.";
            OnApplied?.Invoke();
            Rebuild();
        }

        public void Cancel()
        {
            _draft = _applied != null ? _applied.Clone() : null;
            Close();
        }

        public void Close()
        {
            if (_modal != null && _modal.IsOpen) _modal.CloseSelf();
        }

        public void MarkChanged()
        {
            RefreshFooter();
        }

        public void SetNotice(string notice)
        {
            _notice = notice;
            RefreshFooter();
        }

        public void RefreshBody()
        {
            if (_modal == null || !_modal.Refresh(BodyRegion)) Rebuild();
            RefreshFooter();
        }

        public bool IsExpanded(string pageId, string group)
        {
            return _expanded.TryGetValue(pageId + "/" + group, out bool open) ? open : !CollapsedByDefault.Contains(group) && !CollapsedByDefault.Contains(pageId + "/" + group);
        }

        public void ToggleGroup(string pageId, string group)
        {
            _expanded[pageId + "/" + group] = !IsExpanded(pageId, group);
            RefreshBody();
        }

        public void GroupHeader(UIBuilderContext ui, string pageId, string group)
        {
            bool open = IsExpanded(pageId, group);
            var header = ui.Button(UIMiniHeaderStyleRegistry.FoldoutHeader, group, () => ToggleGroup(pageId, group))
                .TextColor(open ? Color.white : UIMiniHeaderStyleRegistry.Text)
                .Fill();
            if (!header.IsValid) return;
            var arrow = header.GameObject.transform.Find("Arrow");
            if (arrow != null)
            {
                arrow.localEulerAngles = new Vector3(0f, 0f, open ? 0f : 90f);
                var image = arrow.GetComponent<UnityEngine.UI.Image>();
                if (image != null) image.color = open ? UIMiniHeaderStyleRegistry.Accent : UIMiniHeaderStyleRegistry.Sub;
            }
        }
        #endregion

        #region [Private Methods]
        private int IndexOf(string pageId)
        {
            var pages = Pages;
            for (int i = 0; i < pages.Count; i++) if (pages[i].Id == pageId) return i;
            return -1;
        }

        private void Rebuild()
        {
            if (_modal != null) _modal.Build(BuildUI);
        }

        private void HandleClosed()
        {
            KeybindPageView.CancelCapture(this);
            OnClosed?.Invoke();
        }

        private void BuildUI(UIBuilderContext ui)
        {
            var pages = Pages;
            ui.Row(tabs =>
            {
                for (int i = 0; i < pages.Count; i++)
                {
                    int index = i;
                    var tab = tabs.Button(UIMiniHeaderStyleRegistry.TabButton, pages[i].Title, () => ShowTab(index)).Fill();
                    MarkTab(tab, i == _tab);
                }
            }).Gap(0f).Fill();

            ui.Scroll(scroll => scroll.Region(BodyRegion, BuildBody).Padding(0, UIMiniHeaderStyleRegistry.ScrollContentRightPadding, 0, 0).Fill(), 0f).Fill();

            _status = ui.Label(string.Empty).FontSize(13f).TextColor(UIMiniHeaderStyleRegistry.Sub).TextAlign(TextAlignmentOptions.MidlineLeft).Fill().TextComponent;
            ui.Divider(UIMiniHeaderStyleRegistry.Separator, 1f, UIMiniHeaderStyleRegistry.SeparatorColor).Fill();
            ui.Row(footer =>
            {
                footer.Button(UIMiniHeaderStyleRegistry.ButtonSecondary, CurrentPage != null ? CurrentPage.ResetLabel : "Default", ResetCurrentPage);
                footer.Spacer().Fill();
                footer.Button(UIMiniHeaderStyleRegistry.ButtonSecondary, "Cancel", Cancel);
                _applyButton = footer.Button(UIMiniHeaderStyleRegistry.ButtonPrimary, "Apply", Apply);
            }).Gap(UIMiniHeaderStyleRegistry.FooterGap).Align(TextAnchor.MiddleLeft).Fill();
            RefreshFooter();
        }

        private void BuildBody(UIBuilderContext ui)
        {
            var page = CurrentPage;
            if (page == null) return;
            if (page.HasCustomView && SettingsPageViewRegistry.TryGet(page.ViewId, out var view)) view(ui, this);
            else SettingsOptionRows.BuildPage(ui, this, page);
        }

        private void RefreshFooter()
        {
            List<string> changes = PendingChanges;
            if (_applyButton.IsValid) _applyButton.Interactable(changes.Count > 0);
            if (_status == null) return;
            if (changes.Count > 0)
            {
                _status.text = $"Not applied: {string.Join(", ", changes)}";
                _status.color = UIMiniHeaderStyleRegistry.Text;
            }
            else
            {
                _status.text = string.IsNullOrEmpty(_notice) ? "No changes" : _notice;
                _status.color = UIMiniHeaderStyleRegistry.Sub;
            }
            if (changes.Count > 0 && !string.IsNullOrEmpty(_notice)) _status.text = $"{_notice}  {_status.text}";
        }

        private static void MarkTab(UIElementRef tab, bool active)
        {
            if (!tab.IsValid) return;
            var line = tab.GameObject.transform.Find("Active Line");
            var fill = tab.GameObject.transform.Find("Active Fill");
            if (line != null) line.gameObject.SetActive(active);
            if (fill != null) fill.gameObject.SetActive(active);
            tab.TextColor(active ? Color.white : UIMiniHeaderStyleRegistry.Sub);
        }
        #endregion
    }
}
