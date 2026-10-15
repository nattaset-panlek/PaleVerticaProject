using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.Skills.Logic;
using CoreSystem.UI.GameInterface;
using UnityEngine;

namespace CoreSystem.UI
{
    public class UIBuffDisplay : MonoBehaviour
    {
        #region [Variables/Fields]
        public Transform BuffIconDisplayParent;
        public BuffIconUI BuffIconTemplatePrefab;

        private readonly Dictionary<ActiveBuff, BuffIconUI> _activeIcons = new Dictionary<ActiveBuff, BuffIconUI>();
        private readonly List<ActiveBuff> _toRemove = new List<ActiveBuff>();
        private BuffSystem _buffs;
        #endregion

        #region [Unity Methods]
        private void OnEnable()
        {
            PlayerCore.OnPlayerChanged += HandlePlayerChanged;
            HandlePlayerChanged(PlayerCore.GetPlayer());
        }

        private void OnDisable()
        {
            PlayerCore.OnPlayerChanged -= HandlePlayerChanged;
            if (_buffs != null) _buffs.OnBuffsChanged -= Refresh;
            _buffs = null;
        }

        private void Update()
        {
            if (_activeIcons.Count == 0) return;

            foreach (var pair in _activeIcons)
            {
                if (pair.Value != null) pair.Value.SetBuffIcon(pair.Key);
            }
        }
        #endregion

        #region [Private Methods]
        private void HandlePlayerChanged(Character player)
        {
            if (_buffs != null) _buffs.OnBuffsChanged -= Refresh;
            _buffs = player != null ? player.Buffs : null;
            if (_buffs != null) _buffs.OnBuffsChanged += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            _toRemove.Clear();
            foreach (var pair in _activeIcons)
            {
                if (_buffs == null || !Contains(_buffs.ActiveBuffs, pair.Key)) _toRemove.Add(pair.Key);
            }

            foreach (var buff in _toRemove)
            {
                if (_activeIcons[buff] != null) Destroy(_activeIcons[buff].gameObject);
                _activeIcons.Remove(buff);
            }

            if (_buffs == null || BuffIconDisplayParent == null || BuffIconTemplatePrefab == null) return;

            foreach (var buff in _buffs.ActiveBuffs)
            {
                if (_activeIcons.ContainsKey(buff)) continue;

                BuffIconUI icon = Instantiate(BuffIconTemplatePrefab, BuffIconDisplayParent);
                icon.SetBuffIcon(buff);
                _activeIcons.Add(buff, icon);
            }
        }

        private static bool Contains(IReadOnlyList<ActiveBuff> buffs, ActiveBuff buff)
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                if (buffs[i] == buff) return true;
            }
            return false;
        }
        #endregion
    }
}
