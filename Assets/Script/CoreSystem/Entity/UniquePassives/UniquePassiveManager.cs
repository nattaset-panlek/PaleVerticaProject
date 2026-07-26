using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    public class UniquePassiveManager
    {
        #region [Variables/Fields]
        private Character _owner;
        private Dictionary<string, UniquePassive> _activePassives = new Dictionary<string, UniquePassive>();
        private List<UniquePassive> _tempPassiveList = new List<UniquePassive>();
        #endregion

        #region [Public Methods]
        public void Initialize(Character owner)
        {
            _owner = owner;
        }

        public void AddPassive(string id)
        {
            if (_activePassives.ContainsKey(id)) return;
            var passive = CreatePassive(id);
            if (passive != null)
            {
                passive.Initialize(id, _owner);
                _activePassives[id] = passive;
                passive.OnEquip();
            }
        }

        public void AddPassive(UniquePassive passive)
        {
            if (passive == null) return;
            if (_activePassives.TryGetValue(passive.Id, out var existing))
            {
                existing.RemainingDuration = passive.Duration;
                return;
            }
            _activePassives[passive.Id] = passive;
            passive.OnEquip();
        }

        public void RemovePassive(string id)
        {
            if (_activePassives.TryGetValue(id, out var passive))
            {
                passive.OnUnequip();
                _activePassives.Remove(id);
            }
        }

        public bool HasPassive(string id)
        {
            return _activePassives.ContainsKey(id);
        }

        public List<string> ActivePassiveIds => new List<string>(_activePassives.Keys);

        public void Clear()
        {
            foreach (var key in new List<string>(_activePassives.Keys))
            {
                RemovePassive(key);
            }
            _activePassives.Clear();
        }

        public void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            _tempPassiveList.Clear();
            _tempPassiveList.AddRange(_activePassives.Values);
            for (int i = 0; i < _tempPassiveList.Count; i++)
            {
                _tempPassiveList[i].TriggerOnHit(target, ref damage, payload);
            }
        }

        public void TriggerOnTakeDamage(IEntity attacker, ref float damage, SkillPayload payload)
        {
            _tempPassiveList.Clear();
            _tempPassiveList.AddRange(_activePassives.Values);
            for (int i = 0; i < _tempPassiveList.Count; i++)
            {
                _tempPassiveList[i].TriggerOnTakeDamage(attacker, ref damage, payload);
            }
        }

        public void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            _tempPassiveList.Clear();
            _tempPassiveList.AddRange(_activePassives.Values);
            for (int i = 0; i < _tempPassiveList.Count; i++)
            {
                _tempPassiveList[i].TriggerOnPostHit(target, damage, payload);
            }
        }

        public void TriggerOnBasicAttack(IEntity target)
        {
            _tempPassiveList.Clear();
            _tempPassiveList.AddRange(_activePassives.Values);
            for (int i = 0; i < _tempPassiveList.Count; i++)
            {
                _tempPassiveList[i].TriggerOnBasicAttack(target);
            }
        }

        public void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload)
        {
            _tempPassiveList.Clear();
            _tempPassiveList.AddRange(_activePassives.Values);
            for (int i = 0; i < _tempPassiveList.Count; i++)
            {
                _tempPassiveList[i].TriggerOnSkillExecuted(skill, payload);
            }
        }

        public void ModifyCoreAttributes(ref float str, ref float agi, ref float vit, ref float @int, ref float dex, ref float luk)
        {
            _tempPassiveList.Clear();
            _tempPassiveList.AddRange(_activePassives.Values);
            for (int i = 0; i < _tempPassiveList.Count; i++)
            {
                _tempPassiveList[i].ModifyCoreAttributes(ref str, ref agi, ref vit, ref @int, ref dex, ref luk);
            }
        }

        public void ModifyManaCost(ref float manaCost, ref float hpCost, ref bool isHpCost)
        {
            _tempPassiveList.Clear();
            _tempPassiveList.AddRange(_activePassives.Values);
            for (int i = 0; i < _tempPassiveList.Count; i++)
            {
                _tempPassiveList[i].ModifyManaCost(ref manaCost, ref hpCost, ref isHpCost);
            }
        }

        public void Tick(float dt)
        {
            _tempPassiveList.Clear();
            _tempPassiveList.AddRange(_activePassives.Values);
            
            for (int i = 0; i < _tempPassiveList.Count; i++)
            {
                var passive = _tempPassiveList[i];
                passive.Tick(dt);

                if (passive.Duration > 0f)
                {
                    passive.RemainingDuration -= dt;
                    if (passive.RemainingDuration <= 0f)
                    {
                        RemovePassive(passive.Id);
                    }
                }
            }
        }

        public static UniquePassive CreatePassive(string id)
        {
            switch (id)
            {
                case "Vampire": return new VampireUniquePassive();
                case "TimeCollector": return new TimeCollectorUniquePassive();
                case "DepressionChaos": return new DepressionChaosUniquePassive();
                case "Thorn": return new ThornUniquePassive();
                case "ManaLeech": return new ManaLeechUniquePassive();
                case "Execution": return new ExecutionUniquePassive();
                case "BloodBleeding": return new BloodBleedingUniquePassive();
                case "Bleeding": return new BleedingUniquePassive();
                case "CurseOfKanix": return new CurseOfKanixUniquePassive();
                case "BlessingOfMagearna": return new BlessingOfMagearnaUniquePassive();
                case "BloodMage": return new BloodMageUniquePassive();
                case "EldritchFaith": return new EldritchFaithUniquePassive();
                case "Rock": return new RockUniquePassive();
                default: return null;
            }
        }
        #endregion
    }
}
