using CoreSystem.Data;
using CoreSystem.Quests;
using TMPro;
using UnityEngine;

namespace CoreSystem.UI.Quests
{
    public sealed class QuestTrackerWindow : UIWindow
    {
        #region [Variables/Fields]
        public const string WindowName = "QuestTracker";

        [SerializeField] private TMP_Text _text;
        #endregion

        #region [Unity Methods]
        protected override void OnEnable()
        {
            base.OnEnable();
            GameData.Player.Quests.OnQuestsChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            GameData.Player.Quests.OnQuestsChanged -= Refresh;
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            transform.SetAsFirstSibling();
        }
        #endregion

        #region [Private Methods]
        private void Refresh()
        {
            _text.text = QuestTextFormula.Tracker(GameData.Player.Quests, QuestDatabase.All, QuestWindow.NpcName);
        }
        #endregion
    }
}
