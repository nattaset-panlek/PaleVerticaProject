#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections.Generic;
using CoreSystem.Combat;
using CoreSystem.Entity;
using UnityEngine;

namespace ScenarioTesting
{
    public sealed class CombatRecorder
    {
        #region [Variables/Fields]
        public const string HitKind = "hit";
        public const string TakenKind = "taken";
        public const string KillKind = "kill";

        private readonly List<CombatRecordEntry> _entries = new List<CombatRecordEntry>();
        private Character _subject;
        #endregion

        #region [Properties]
        public IReadOnlyList<CombatRecordEntry> Entries => _entries;
        public Character Subject => _subject;
        public float DamageDealt => Sum(HitKind);
        public float DamageTaken => Sum(TakenKind);
        public int Hits => Count(HitKind);
        public int Kills => Count(KillKind);
        #endregion

        #region [Public Methods]
        public void Attach(Character subject)
        {
            Detach();
            _subject = subject;
            if (_subject == null) return;
            _subject.OnHitTarget += HandleHit;
            _subject.OnTakeDamage += HandleTaken;
            CombatEvents.OnKilled += HandleKilled;
        }

        public void Detach()
        {
            if (_subject != null)
            {
                _subject.OnHitTarget -= HandleHit;
                _subject.OnTakeDamage -= HandleTaken;
            }
            CombatEvents.OnKilled -= HandleKilled;
            _subject = null;
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public float DamageDealtTo(IEntity target)
        {
            string name = NameOf(target);
            float total = 0f;
            foreach (CombatRecordEntry entry in _entries)
            {
                if (entry.Kind == HitKind && entry.Target == name) total += entry.Amount;
            }
            return total;
        }
        #endregion

        #region [Private Methods]
        private void HandleHit(IEntity target, float damage)
        {
            Add(HitKind, _subject, target, damage);
        }

        private void HandleTaken(IEntity attacker, float damage)
        {
            Add(TakenKind, attacker, _subject, damage);
        }

        private void HandleKilled(IEntity killer, IEntity victim)
        {
            if (ReferenceEquals(killer, _subject)) Add(KillKind, killer, victim, 1f);
        }

        private void Add(string kind, IEntity source, IEntity target, float amount)
        {
            _entries.Add(new CombatRecordEntry { Kind = kind, Source = NameOf(source), Target = NameOf(target), Amount = amount, Time = UnityEngine.Time.time });
        }

        private float Sum(string kind)
        {
            float total = 0f;
            foreach (CombatRecordEntry entry in _entries) if (entry.Kind == kind) total += entry.Amount;
            return total;
        }

        private int Count(string kind)
        {
            int total = 0;
            foreach (CombatRecordEntry entry in _entries) if (entry.Kind == kind) total++;
            return total;
        }

        public static string NameOf(IEntity entity)
        {
            if (entity == null || (entity is Object unityObject && unityObject == null)) return "none";
            return entity is Component component ? $"{component.name}#{component.GetInstanceID()}" : entity.ToString();
        }
        #endregion
    }
}
#endif
