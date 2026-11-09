using TMPro;
using UnityEngine;

namespace CoreSystem.UI.Quests
{
    public sealed class QuestObjectiveRow : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _value;
        #endregion

        #region [Public Methods]
        public void Bind(string label, string value, Color color)
        {
            _label.text = label;
            _value.text = value;
            _label.color = color;
            _value.color = color;
        }
        #endregion
    }
}
