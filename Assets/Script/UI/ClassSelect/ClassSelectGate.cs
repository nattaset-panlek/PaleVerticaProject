using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.SkillTree;
using CoreSystem.UI.SystemMenu;
using UnityEngine;

namespace CoreSystem.UI.ClassSelect
{
    public static class ClassSelectGate
    {
        #region [Variables/Fields]
        public const string SkillTreeWindowName = "CharacterSkillTree";

        private static readonly SystemConfirmDialog Notice = new SystemConfirmDialog();
        #endregion

        #region [Public Methods]
        public static string Resolve(string windowName)
        {
            if (windowName != SkillTreeWindowName) return windowName;

            Character player = PlayerCore.GetPlayer();
            if (player == null || GameData.Player.Class.HasClass) return windowName;
            if (player.JobLevel >= PassiveClassFormula.RequiredJobLevel) return ClassSelectModal.WindowName;

            Notice.Show("Class Mastery", $"Reach Job Level {PassiveClassFormula.RequiredJobLevel} to choose your class.", "OK", null);
            return null;
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Attach()
        {
            UIManager.OpenRedirect = Resolve;
            PlayerCore.OnReady += core =>
            {
                if (core.Controlled != null) Bind(core.Controlled);
                core.OnPossessed += Bind;
                core.OnReleased += Unbind;
            };
        }

        private static void Bind(Character character)
        {
            if (character == null || character.expSystem == null) return;
            character.expSystem.OnJobLevelUp -= HandleJobLevelUp;
            character.expSystem.OnJobLevelUp += HandleJobLevelUp;
        }

        private static void Unbind(Character character)
        {
            if (character != null && character.expSystem != null) character.expSystem.OnJobLevelUp -= HandleJobLevelUp;
        }

        private static void HandleJobLevelUp(int level)
        {
            if (level != PassiveClassFormula.RequiredJobLevel || GameData.Player.Class.HasClass) return;
            if (UIManager.Instance != null) UIManager.Instance.Open(ClassSelectModal.WindowName);
        }
        #endregion
    }
}
