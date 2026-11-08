using CoreSystem.Combat;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Summon
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class SummonedUnit : MonoBehaviour
    {
        #region [Variables/Fields]
        private Character _owner;
        private Character _body;
        private int _skillId;
        private float _spawnTime;
        private bool _isExpiring;
        #endregion

        #region [Properties]
        public Character Owner => _owner;
        public Character Body => _body;
        public int SkillId => _skillId;
        public float SpawnTime => _spawnTime;
        public bool IsExpiring => _isExpiring;
        public bool IsReady => _owner != null && _body != null;
        public event System.Action<SummonedUnit> OnReady;
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
            SummonRegistry.Unregister(this);
            TeamRegistry.Unbind(_body);
        }
        #endregion

        #region [Public Methods]
        public void Bind(Character owner, Character body, int skillId, float lifetime)
        {
            _owner = owner;
            _body = body;
            _skillId = skillId;
            _spawnTime = Time.time;
            SummonRegistry.Register(this);
            TeamRegistry.Bind(_body, _owner);
            if (lifetime > 0f) Destroy(gameObject, lifetime);
            OnReady?.Invoke(this);
        }

        public void Dismiss()
        {
            if (_isExpiring) return;
            _isExpiring = true;
            SummonRegistry.Unregister(this);
            Destroy(gameObject);
        }
        #endregion

        #region [Private Methods]
        private void HandleKilled(IEntity killer, IEntity victim)
        {
            if (_owner != null && ReferenceEquals(victim, _owner)) Dismiss();
        }
        #endregion
    }
}
