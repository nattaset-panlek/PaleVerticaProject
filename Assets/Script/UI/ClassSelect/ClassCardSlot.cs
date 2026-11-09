using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.ClassSelect
{
    public class ClassCardSlot : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private string _classLabel;
        [SerializeField] private Button _button;

        public event Action<ClassCardSlot> OnClicked;
        #endregion

        #region [Properties]
        public string ClassLabel => _classLabel;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _button.onClick.AddListener(() => OnClicked?.Invoke(this));
        }
        #endregion
    }
}
