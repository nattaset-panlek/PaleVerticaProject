using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.Settings;
using CoreSystem.Skills;
using UnityEngine;

namespace CoreSystem.UI.ActionProgress
{
    public static class CastProgressEvents
    {
        #region [Variables/Fields]
        public const string CastActionType = "CAST";

        private static SkillCaster _caster;
        #endregion

        #region [Properties]
        public static bool IsBound => _caster != null;
        #endregion

        #region [Public Methods]
        public static void Bind(Character character)
        {
            Unbind();
            if (character == null || character.skillCaster == null) return;
            _caster = character.skillCaster;
            _caster.OnCastInitiated += HandleCastInitiated;
            _caster.OnCastProgress += HandleCastProgress;
            _caster.OnCastExecuted += HandleCastExecuted;
            _caster.OnCastInterrupted += HandleCastInterrupted;
        }

        public static void Unbind()
        {
            if (_caster == null) return;
            _caster.OnCastInitiated -= HandleCastInitiated;
            _caster.OnCastProgress -= HandleCastProgress;
            _caster.OnCastExecuted -= HandleCastExecuted;
            _caster.OnCastInterrupted -= HandleCastInterrupted;
            ActionProgressWindow.Cancel(_caster);
            _caster = null;
        }

        public static ActionProgressRequest BuildRequest(SkillCaster caster, string skillName)
        {
            SkillInstance instance = caster != null ? caster.ActiveSkillInstance : null;
            return new ActionProgressRequest
            {
                ActionName = skillName,
                ActionType = CastActionType,
                Icon = instance != null && instance.SkillData != null ? instance.SkillData.Icon : null,
                Color = ActionProgressColor.Magenta,
                Owner = caster
            };
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            PlayerCore.OnPlayerChanged -= Bind;
            PlayerCore.OnPlayerChanged += Bind;
            GameSetting.OnChanged -= HandleSettingChanged;
            GameSetting.OnChanged += HandleSettingChanged;
            Bind(PlayerCore.GetPlayer());
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _caster = null;
        }

        private static void HandleSettingChanged()
        {
            if (!GameSetting.ShowCastProgress) ActionProgressWindow.Cancel(_caster);
        }

        private static void HandleCastInitiated(string skillName, float totalSeconds)
        {
            if (!GameSetting.ShowCastProgress || totalSeconds <= 0f) return;
            ActionProgressWindow.Show(BuildRequest(_caster, skillName));
            ActionProgressWindow.Report(_caster, 0f, totalSeconds);
        }

        private static void HandleCastProgress(float elapsed, float total)
        {
            if (total <= 0f) return;
            ActionProgressWindow.Report(_caster, elapsed / total, total - elapsed);
        }

        private static void HandleCastExecuted(string skillName)
        {
            ActionProgressWindow.Complete(_caster);
        }

        private static void HandleCastInterrupted()
        {
            ActionProgressWindow.Cancel(_caster);
        }
        #endregion
    }
}
