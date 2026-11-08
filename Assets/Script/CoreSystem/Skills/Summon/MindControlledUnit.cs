using CoreSystem.Agent;
using CoreSystem.Combat;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Summon
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class MindControlledUnit : MonoBehaviour
    {
        #region [Variables/Fields]
        private Character _owner;
        private Character _body;
        private int _originalFaction;
        private bool _isActive;
        #endregion

        #region [Unity Methods]
        private void OnEnable()
        {
            CombatEvents.OnKilled += HandleKilled;
        }

        private void OnDisable()
        {
            CombatEvents.OnKilled -= HandleKilled;
        }

        private void OnDestroy()
        {
            Release();
        }
        #endregion

        #region [Public Methods]
        public void Begin(Character owner, Character body, float duration)
        {
            if (!_isActive) _originalFaction = body.faction;
            _owner = owner;
            _body = body;
            _isActive = true;
            body.faction = owner.faction;
            TeamRegistry.Bind(body, owner);
            ResetBrain();

            CancelInvoke(nameof(End));
            Invoke(nameof(End), duration);
        }
        #endregion

        #region [Private Methods]
        private void End()
        {
            Release();
            ResetBrain();
            Destroy(this);
        }

        private void Release()
        {
            if (!_isActive) return;
            _isActive = false;
            TeamRegistry.Unbind(_body);
            if (_body != null) _body.faction = _originalFaction;
        }

        private void ResetBrain()
        {
            if (_body == null || AgentCore.Instance == null) return;
            AgentCore.Instance.GetHandle(_body)?.ForceReset();
        }

        private void HandleKilled(IEntity killer, IEntity victim)
        {
            if (_owner != null && ReferenceEquals(victim, _owner)) End();
        }
        #endregion
    }
}
