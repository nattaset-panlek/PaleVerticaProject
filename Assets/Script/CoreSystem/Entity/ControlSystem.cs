using System.Collections.Generic;
using CoreSystem.Skills;
using UnityEngine;

namespace CoreSystem.Entity
{
    [System.Serializable]
    public class ControlSystem
    {
        #region [Variables/Fields]
        private Character owner;
        private readonly List<ControlLease> _leases = new List<ControlLease>();
        private object _lastHolder;
        #endregion

        #region [Properties]
        public System.Func<ControlLane, bool> ClaimFilter { get; set; }
        public IReadOnlyList<ControlLease> Leases => _leases;
        public ControlLease ActiveLease => FindActiveLease();
        public object CurrentHolder => ActiveLease?.Holder;
        public bool IsOverridden => ActiveLease != null && ActiveLease.Lane > ControlLane.Owner;
        public event System.Action<object, object> OnHolderChanged;
        #endregion

        #region [Public Methods]
        public void Initialize(Character owner)
        {
            this.owner = owner;
        }

        public ControlLease Claim(object holder, ControlLane lane, float duration = -1f, System.Func<bool> keepWhile = null, System.Action onEnded = null)
        {
            if (holder == null) return null;
            if (ClaimFilter != null && !ClaimFilter(lane)) return null;

            for (int i = 0; i < _leases.Count; i++)
            {
                var lease = _leases[i];
                if (lease.Lane == lane && lease.Holder != holder) return null;
                if (lease.Lane == lane && lease.Holder == holder)
                {
                    lease.RemainingTime = duration;
                    return lease;
                }
            }

            var created = new ControlLease(holder, lane, duration, keepWhile, onEnded);
            _leases.Add(created);
            NotifyIfHolderChanged();
            return created;
        }

        public ControlLease TransferOwnership(object newOwner)
        {
            for (int i = _leases.Count - 1; i >= 0; i--)
            {
                if (_leases[i].Lane == ControlLane.Owner && _leases[i].Holder != newOwner) End(_leases[i]);
            }
            return Claim(newOwner, ControlLane.Owner);
        }

        public bool Release(ControlLease lease)
        {
            if (lease == null || !_leases.Contains(lease)) return false;
            End(lease);
            return true;
        }

        public void Release(object holder)
        {
            for (int i = _leases.Count - 1; i >= 0; i--)
            {
                if (_leases[i].Holder == holder) End(_leases[i]);
            }
        }

        public void ReleaseAllAbove(ControlLane lane)
        {
            for (int i = _leases.Count - 1; i >= 0; i--)
            {
                if (_leases[i].Lane > lane) End(_leases[i]);
            }
        }

        public bool CanCommand(object sender)
        {
            var active = ActiveLease;
            return active == null || active.Holder == sender;
        }

        public bool IsHeldBy(object holder)
        {
            return ActiveLease != null && ActiveLease.Holder == holder;
        }

        public void Tick(float deltaTime)
        {
            for (int i = _leases.Count - 1; i >= 0; i--)
            {
                if (i >= _leases.Count) continue;
                var lease = _leases[i];
                if (lease.IsTimed)
                {
                    lease.RemainingTime -= deltaTime;
                    if (lease.RemainingTime <= 0f) { End(lease); continue; }
                }
                if (lease.KeepWhile != null && !lease.KeepWhile()) End(lease);
            }
        }

        public bool RequestOverride(object requester)
        {
            return Claim(requester, ControlLane.Possession) != null;
        }

        public void ReleaseOverride(object requester)
        {
            for (int i = _leases.Count - 1; i >= 0; i--)
            {
                if (_leases[i].Holder == requester && _leases[i].Lane > ControlLane.Owner) End(_leases[i]);
            }
        }

        public void ForceClearOverride()
        {
            ReleaseAllAbove(ControlLane.Owner);
        }

        public void CommandMove(Vector3 direction, object sender = null)
        {
            if (owner == null || owner.movementSystem == null) return;
            if (!CanCommand(sender)) return;
            owner.movementSystem.Move(direction);
        }

        public void CommandBasicAttack(IEntity target, object sender = null)
        {
            if (owner == null) return;
            if (!CanCommand(sender)) return;
            owner.BasicAttack(target);
        }

        public void CommandCastSkill(SkillInstance skill, Vector3 targetPos, bool isPlayer = false, object sender = null)
        {
            if (owner == null || owner.skillCaster == null) return;
            if (!CanCommand(sender)) return;
            owner.skillCaster.CastSkill(ResolveChain(skill, null), targetPos, isPlayer);
        }

        public void CommandCastSkill(SkillInstance skill, SkillPayload payload, object sender = null)
        {
            if (owner == null || owner.skillCaster == null) return;
            if (!CanCommand(sender)) return;
            owner.skillCaster.CastSkill(ResolveChain(skill, payload), payload);
        }

        public void CommandInterruptCast(object sender = null)
        {
            if (owner == null || owner.skillCaster == null) return;
            if (!CanCommand(sender)) return;
            owner.skillCaster.InterruptCast();
        }
        #endregion

        #region [Private Methods]
        private ControlLease FindActiveLease()
        {
            ControlLease best = null;
            for (int i = 0; i < _leases.Count; i++)
            {
                if (best == null || _leases[i].Lane >= best.Lane) best = _leases[i];
            }
            return best;
        }

        private void End(ControlLease lease)
        {
            if (!_leases.Remove(lease)) return;
            lease.IsActive = false;
            lease.OnEnded?.Invoke();
            NotifyIfHolderChanged();
        }

        private void NotifyIfHolderChanged()
        {
            var current = CurrentHolder;
            if (current == _lastHolder) return;
            var previous = _lastHolder;
            _lastHolder = current;
            OnHolderChanged?.Invoke(previous, current);
        }

        private SkillInstance ResolveChain(SkillInstance skill, SkillPayload payload)
        {
            if (skill == null || skill.SkillData == null) return skill;

            var overrideData = ActiveSkill.GetActiveChainOverride(owner, skill.SkillData);
            if (overrideData == null || overrideData.ReplacementSkill == null) return skill;

            var replacement = owner.skillCaster.GrantSkill(overrideData.ReplacementSkill);
            if (overrideData.ConsumeBuffOnCast)
            {
                var buffs = owner.GetComponent<CoreSystem.Skills.Logic.BuffSystem>();
                if (buffs != null) buffs.RemoveBuff(overrideData.RequiredBuffCode);
            }
            if (payload != null) payload.SkillID = replacement.SkillData.Id;
            return replacement;
        }
        #endregion
    }
}
