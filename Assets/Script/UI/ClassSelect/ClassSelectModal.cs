using System;
using CoreSystem.Data;
using CoreSystem.SkillTree;
using CoreSystem.SkillTree.Models;
using CoreSystem.UI.SystemMenu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreSystem.UI.ClassSelect
{
    public class ClassSelectModal : UIModal
    {
        #region [Variables/Fields]
        public new const string WindowName = "ClassSelect";

        [SerializeField] private ClassCardSlot[] _cards;
        [SerializeField] private Button _closeButton;

        private readonly SystemConfirmDialog _confirm = new SystemConfirmDialog();
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();
            if (_closeButton != null) _closeButton.onClick.AddListener(CloseSelfView);
            foreach (ClassCardSlot card in _cards) card.OnClicked += HandleCardClicked;
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
        #endregion

        #region [Private Methods]
        private void HandleCardClicked(ClassCardSlot card)
        {
            SkillTreeNode origin = FindOrigin(card.ClassLabel);
            if (origin == null)
            {
                Debug.LogError($"[ClassSelect] No origin node labelled {card.ClassLabel} in the Skill Tree.");
                return;
            }

            _confirm.Show(
                "Class Mastery",
                $"Begin your path as {card.ClassLabel}?",
                "Confirm",
                () => Choose(origin.id));
        }

        private void Choose(int originNodeId)
        {
            if (!GameData.Player.Class.ChooseClass(originNodeId)) return;
            UIManager.Instance.Close(WindowName);
            UIManager.Instance.Open(ClassSelectGate.SkillTreeWindowName);
        }

        private void CloseSelfView()
        {
            UIManager.Instance.Close(WindowName);
        }

        private static SkillTreeNode FindOrigin(string label)
        {
            return SkillTreeDatabase.GetBaseClassNodes().Find(n => string.Equals(n.label, label, StringComparison.OrdinalIgnoreCase));
        }
        #endregion
    }
}
