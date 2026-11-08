using System.Linq;
using CoreSystem.Settings;
using CoreSystem.Settings.Keybinds;
using CoreSystem.Settings.Options;
using CoreSystem.UI.SystemMenu;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreSystem.Tests
{
    public class SystemMenuTests
    {
        #region [Variables/Fields]
        private const string FrameLimitKey = "Setting.Display.FrameLimit";
        private const string MasterKey = "Setting.Audio.Master";
        private bool _hadFrameLimit;
        private int _frameLimit;
        private bool _hadMaster;
        private float _master;
        private string _store;
        #endregion

        #region [Public Methods]
        [SetUp]
        public void SetUp()
        {
            _hadFrameLimit = PlayerPrefs.HasKey(FrameLimitKey);
            _frameLimit = PlayerPrefs.GetInt(FrameLimitKey);
            _hadMaster = PlayerPrefs.HasKey(MasterKey);
            _master = PlayerPrefs.GetFloat(MasterKey);
            _store = SettingsStorage.Export();
            SettingsRegistry.ResetToCatalog();
        }

        [TearDown]
        public void TearDown()
        {
            if (_hadFrameLimit) PlayerPrefs.SetInt(FrameLimitKey, _frameLimit); else PlayerPrefs.DeleteKey(FrameLimitKey);
            if (_hadMaster) PlayerPrefs.SetFloat(MasterKey, _master); else PlayerPrefs.DeleteKey(MasterKey);
            SettingsStorage.Import(_store);
            SettingsStorage.Save();
            SettingsRegistry.ResetToCatalog();
            AudioSetting.Apply();
        }

        [Test]
        public void MenuItems_FollowTheDesignOrder()
        {
            var labels = new SystemMenuWindow().Items.Where(i => !i.IsSeparator).Select(i => i.Label).ToArray();
            CollectionAssert.AreEqual(new[] { "Character", "Equipment", "Inventory", "Skills", "Passive Tree", "Keybind", "Settings", "Save Game", "Load Game", "Return to Title", "Exit Game" }, labels);
            Assert.AreEqual(3, new SystemMenuWindow().Items.Count(i => i.IsSeparator));
        }

        [Test]
        public void Keybind_MenuItemIsEnabled()
        {
            var keybind = new SystemMenuWindow().Items.First(i => i.Label == "Keybind");
            Assert.IsTrue(keybind.Enabled);
        }

        [Test]
        public void MenuItem_WithoutAction_IsDisabled()
        {
            Assert.IsFalse(new SystemMenuItem("X", null).Enabled);
            Assert.IsTrue(new SystemMenuItem("X", () => { }).Enabled);
            Assert.IsFalse(SystemMenuItem.Separator.Enabled);
        }

        [Test]
        public void FrameLimit_RejectsValuesNotInTheList()
        {
            DisplaySetting.FrameLimit = 144;
            DisplaySetting.FrameLimit = 77;
            Assert.AreEqual(144, DisplaySetting.FrameLimit);
        }

        [Test]
        public void FrameLimit_ZeroMeansUnlimited()
        {
            Assert.AreEqual("Unlimited", DisplaySetting.FormatFrameLimit(0));
            Assert.AreEqual("60", DisplaySetting.FormatFrameLimit(60));
            Assert.Contains(0, DisplaySetting.FrameLimits);
        }

        [Test]
        public void Volume_IsClampedAndMasterDrivesListener()
        {
            SettingsStorage.SetBool(SettingKeys.MuteMaster, false);
            AudioSetting.SetVolume(AudioChannel.Master, 1.5f);
            Assert.AreEqual(1f, AudioSetting.GetVolume(AudioChannel.Master));
            Assert.AreEqual(1f, AudioListener.volume, 0.0001f);
            AudioSetting.SetVolume(AudioChannel.Master, -1f);
            Assert.AreEqual(0f, AudioSetting.GetVolume(AudioChannel.Master));
        }

        [Test]
        public void Mute_SilencesTheChannelButKeepsTheVolume()
        {
            AudioSetting.SetVolume(AudioChannel.Music, 0.6f);
            SettingsStorage.SetBool(AudioSetting.MuteKey(AudioChannel.Music), true);
            Assert.AreEqual(0f, AudioSetting.GetEffectiveVolume(AudioChannel.Music));
            Assert.AreEqual(0.6f, AudioSetting.GetVolume(AudioChannel.Music), 0.0001f);
        }

        [Test]
        public void Pages_FollowTheDesignTabs()
        {
            CollectionAssert.AreEqual(new[] { "Graphics", "Audio", "Gameplay", "Controls" }, SettingsRegistry.Pages.Select(p => p.Title).ToArray());
            Assert.AreEqual(SettingKeys.KeybindView, SettingsRegistry.FindPage(SettingKeys.PageControls).ViewId);
            Assert.AreEqual(4, new SettingsWindow().Pages.Count);
        }

        [Test]
        public void Catalog_HasEveryFigmaOption()
        {
            string[] keys =
            {
                SettingKeys.Resolution, SettingKeys.DisplayMode, SettingKeys.VSync, SettingKeys.FrameLimit, SettingKeys.Brightness, SettingKeys.Quality,
                SettingKeys.VolumeMaster, SettingKeys.VolumeMusic, SettingKeys.VolumeEffects, SettingKeys.VolumeVoice, SettingKeys.VolumeInterface,
                SettingKeys.MuteMaster, SettingKeys.MuteMusic, SettingKeys.MuteEffects, SettingKeys.MuteVoice, SettingKeys.MuteInterface, SettingKeys.MuteInBackground,
                SettingKeys.CameraShake, SettingKeys.ZoomSpeed, SettingKeys.ShowDamageNumbers, SettingKeys.ShowMinimap, SettingKeys.HudScale, SettingKeys.Language
            };
            foreach (string key in keys) Assert.IsNotNull(SettingsRegistry.Find(key), key);
            Assert.AreEqual(SettingKeys.VolumeMusic, SettingsRegistry.Find(SettingKeys.MuteMusic).InlineWithKey);
            CollectionAssert.AreEqual(new[] { SettingKeys.GroupCamera, SettingKeys.GroupInterface, SettingKeys.GroupLanguage }, SettingsRegistry.GetGroups(SettingKeys.PageGameplay));
        }

        [Test]
        public void Registry_AcceptsNewPagesAndOptions()
        {
            SettingsRegistry.AddPage(new SettingsPage("test.page", "Test", 99));
            SettingsRegistry.AddOption("test.page", null, "Test Toggle", new ToggleSettingOption("test.toggle", true));

            Assert.AreEqual("Test", SettingsRegistry.Pages.Last().Title);
            Assert.AreEqual(1, SettingsRegistry.GetOptions("test.page").Count);
            Assert.AreEqual(true, SettingsDraft.Capture().Get("test.toggle"));

            Assert.IsTrue(SettingsRegistry.RemovePage("test.page"));
            Assert.IsNull(SettingsRegistry.Find("test.toggle"));
        }

        [Test]
        public void Storage_RoundTripsThroughJson()
        {
            SettingsStorage.SetFloat("test.float", 0.25f);
            SettingsStorage.SetBool("test.bool", true);
            SettingsStorage.SetString("test.string", "a,b");
            string json = SettingsStorage.Export();

            SettingsStorage.ClearAll();
            Assert.IsFalse(SettingsStorage.Has("test.float"));

            SettingsStorage.Import(json);
            Assert.AreEqual(0.25f, SettingsStorage.GetFloat("test.float", 0f));
            Assert.IsTrue(SettingsStorage.GetBool("test.bool", false));
            Assert.AreEqual("a,b", SettingsStorage.GetString("test.string", null));
            Assert.AreEqual(SettingsStorage.Version, SettingsStorage.LoadedVersion);
        }

        [Test]
        public void Storage_BadJsonFallsBackToDefaults()
        {
            SettingsStorage.Import("not json");
            Assert.AreEqual(7, SettingsStorage.GetInt("missing", 7));
        }

        [Test]
        public void Slider_ClampsAndRounds()
        {
            var slider = new SliderSettingOption("test.slider", 0f, 100f, 50f);
            Assert.AreEqual(100f, (float)slider.Normalize(140f));
            Assert.AreEqual(3f, (float)slider.Normalize(2.6f));
            Assert.AreEqual(50f, (float)slider.Normalize("bad"));
        }

        [Test]
        public void Draft_ChangesStayPendingUntilApply()
        {
            DisplaySetting.FrameLimit = 60;
            var applied = SettingsDraft.Capture();
            var draft = applied.Clone();

            draft.Set(SettingKeys.FrameLimit, DisplaySetting.FrameLimits.ToList().IndexOf(144));
            draft.Set(SettingKeys.VSync, !applied.Get<bool>(SettingKeys.VSync));

            Assert.AreEqual(60, DisplaySetting.FrameLimit);
            CollectionAssert.AreEquivalent(new[] { "Vertical Sync", "Frame Rate Limit" }, draft.Differences(applied));
            Assert.IsEmpty(applied.Differences(applied.Clone()));

            draft.Set(SettingKeys.VSync, applied.Get<bool>(SettingKeys.VSync));
            draft.Apply();
            Assert.AreEqual(144, DisplaySetting.FrameLimit);
            Assert.IsEmpty(SettingsDraft.Capture().Differences(draft));
        }

        [Test]
        public void Draft_ApplyRaisesChangedPerKey()
        {
            var draft = SettingsDraft.Capture();
            draft.Set(SettingKeys.CameraShake, !draft.Get<bool>(SettingKeys.CameraShake));
            string raised = null;
            void Handle(string key) => raised = key;
            SettingsEvents.OnChanged += Handle;
            try { draft.Apply(); }
            finally { SettingsEvents.OnChanged -= Handle; }
            Assert.AreEqual(SettingKeys.CameraShake, raised);
        }

        [Test]
        public void Draft_ResetPage_OnlyChangesTheDraft()
        {
            AudioSetting.SetVolume(AudioChannel.Master, 0.3f);
            var draft = SettingsDraft.Capture();

            draft.ResetPage(SettingKeys.PageAudio);

            Assert.AreEqual(AudioSetting.DefaultVolume * 100f, draft.Get<float>(SettingKeys.VolumeMaster), 0.01f);
            Assert.AreEqual(0.3f, AudioSetting.GetVolume(AudioChannel.Master), 0.0001f);
        }

        [Test]
        public void Draft_InlineOptionsUseTheParentLabel()
        {
            var draft = SettingsDraft.Capture();
            var applied = draft.Clone();
            draft.Set(SettingKeys.MuteMusic, !applied.Get<bool>(SettingKeys.MuteMusic));
            CollectionAssert.AreEqual(new[] { "Music Mute" }, draft.Differences(applied));
        }

        [Test]
        public void Keybinds_DefaultsComeFromTheInputAsset()
        {
            var up = MoveUp();
            Assert.IsNotNull(up);
            Assert.AreEqual("Movement", up.Group);
            Assert.AreEqual(Key.W, up.Default.Primary);
            Assert.AreEqual(Key.UpArrow, up.Default.Secondary);
            Assert.AreEqual(Key.Digit1, KeybindSetting.Find(KeybindSetting.HotbarId(0)).Default.Primary);
            Assert.AreEqual(Key.Digit0, KeybindSetting.Find(KeybindSetting.HotbarId(9)).Default.Primary);
        }

        [Test]
        public void Keybinds_AssignSwapsOnConflict()
        {
            var map = KeybindSetting.CaptureDefaults();
            string up = MoveUp().Id;
            string down = KeybindSetting.Definitions.First(d => d.ActionName == "Move" && d.PartName == "down").Id;

            bool conflict = KeybindSetting.Assign(map, up, 0, Key.S, out string otherId, out int otherSlot);

            Assert.IsTrue(conflict);
            Assert.AreEqual(down, otherId);
            Assert.AreEqual(0, otherSlot);
            Assert.AreEqual(Key.S, map[up].Primary);
            Assert.AreEqual(Key.W, map[down].Primary);
        }

        [Test]
        public void Keybinds_DraftTracksChangesAndReset()
        {
            var applied = SettingsDraft.Capture();
            var draft = applied.Clone();
            KeybindSetting.Assign(draft.Keybinds, MoveUp().Id, 0, Key.I, out _, out _);
            CollectionAssert.Contains(draft.Differences(applied), SettingsDraft.KeybindsLabel);

            draft.ResetPage(SettingKeys.PageControls);
            Assert.AreEqual(Key.W, draft.Keybinds[MoveUp().Id].Primary);
        }

        [Test]
        public void Keybinds_ApplyOverridesTheInputAsset()
        {
            var map = KeybindSetting.CaptureDefaults();
            var up = MoveUp();
            KeybindSetting.Assign(map, up.Id, 0, Key.I, out _, out _);
            KeybindSetting.SetAll(map);
            Assert.AreEqual(Key.I, KeybindSetting.GetPair(up.Id).Primary);

            var actions = new InputSystem_Actions();
            try
            {
                KeybindSetting.ApplyTo(actions.asset);
                var move = actions.asset.FindAction("Player/Move");
                int index = KeybindSetting.KeyboardBindingIndices(move, "up")[0];
                Assert.AreEqual(Key.I, KeybindSetting.FromControlPath(move.bindings[index].effectivePath));
            }
            finally
            {
                Object.DestroyImmediate(actions.asset);
            }
        }

        [Test]
        public void Keybinds_ControlPathsRoundTrip()
        {
            Assert.AreEqual("1", KeybindSetting.ControlName(Key.Digit1));
            Assert.AreEqual("leftShift", KeybindSetting.ControlName(Key.LeftShift));
            Assert.AreEqual(Key.Digit1, KeybindSetting.FromControlPath(KeybindSetting.ToControlPath(Key.Digit1)));
            Assert.AreEqual(Key.LeftShift, KeybindSetting.FromControlPath(KeybindSetting.ToControlPath(Key.LeftShift)));
            Assert.AreEqual("SHIFT", KeybindSetting.DisplayName(Key.LeftShift));
        }
        #endregion

        #region [Private Methods]
        private static KeybindDefinition MoveUp()
        {
            KeybindSetting.EnsureDefaults();
            return KeybindSetting.Definitions.FirstOrDefault(d => d.ActionName == "Move" && d.PartName == "up");
        }
        #endregion
    }
}
