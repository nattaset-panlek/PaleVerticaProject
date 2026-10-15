using UnityEngine;
using UnityEngine.UI;
using CoreSystem.Skills.Logic;
using System.Text;
using System.Collections.Generic;

namespace CoreSystem.UI
{
    public class UIBuffDisplay : MonoBehaviour
    {
        public BuffSystem BuffManager;
        public Transform BuffIconDisplayParent;
        public BuffIconUI BuffIconTemplatePrefab;

        private Dictionary<ActiveBuff, BuffIconUI> _activeIcons = new Dictionary<ActiveBuff, BuffIconUI>();

        private void Update()
        {
            if (BuffManager == null || BuffIconDisplayParent == null || BuffIconTemplatePrefab == null) return;

            // 1. Remove icons for buffs that are no longer active
            List<ActiveBuff> toRemove = new List<ActiveBuff>();
            foreach (var kvp in _activeIcons)
            {
                bool stillActive = false;
                foreach (var activeBuff in BuffManager.ActiveBuffs)
                {
                    if (activeBuff == kvp.Key)
                    {
                        stillActive = true;
                        break;
                    }
                }

                if (!stillActive)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var buff in toRemove)
            {
                if (_activeIcons[buff] != null)
                {
                    Destroy(_activeIcons[buff].gameObject);
                }
                _activeIcons.Remove(buff);
            }

            // 2. Add icons for new buffs and update all
            foreach (var buff in BuffManager.ActiveBuffs)
            {
                if (!_activeIcons.ContainsKey(buff))
                {
                    BuffIconUI newIcon = Instantiate(BuffIconTemplatePrefab, BuffIconDisplayParent);
                    _activeIcons.Add(buff, newIcon);
                }

                // Update values
                _activeIcons[buff].SetBuffIcon(buff);
            }
        }
    }
}
