using System.Collections.Generic;
using CoreSystem.Attributes;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    public enum BuffCategory
    {
        Aura,
        Attribute,
        Skill,
        Effect,
        Unique,
        Default
    }

    public class ActiveBuff
    {
        public string BuffName;
        public string BuffCode;
        public List<AttributeModifier> Modifiers = new List<AttributeModifier>();
        public AttributeType TargetAttribute;
        public float RemainingDuration;
        public float TotalDuration;
        public Sprite Icon;
        public BuffCategory Category;
        public int MaxStacks = 1;
        public int Stacks => Modifiers.Count;
        public float TickTimer;
        public float TickInterval;
        public System.Action OnTick;
    }

    public class TemporaryBuffManager : MonoBehaviour
    {
        private AttributeSystem attributeSystem;
        private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

        public IReadOnlyList<ActiveBuff> ActiveBuffs => activeBuffs;
        
        public event System.Action OnBuffsChanged;

        private void Awake()
        {
            var entity = GetComponent<Entity.IEntity>();
            if (entity != null)
            {
                attributeSystem = entity.attributeSystem;
            }
        }
        
        public bool HasBuff(string buffCode)
        {
            for (int i = 0; i < activeBuffs.Count; i++)
            {
                if (activeBuffs[i].BuffCode == buffCode) return true;
            }
            return false;
        }

        public void RemoveBuff(string buffCode)
        {
            bool removed = false;
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                if (activeBuffs[i].BuffCode == buffCode)
                {
                    if (attributeSystem != null)
                    {
                        foreach (var mod in activeBuffs[i].Modifiers)
                        {
                            attributeSystem.RemoveModifier(activeBuffs[i].TargetAttribute, mod);
                        }
                    }
                    activeBuffs.RemoveAt(i);
                    removed = true;
                }
            }
            
            if (removed)
            {
                OnBuffsChanged?.Invoke();
            }
        }

        public void ApplyTemporaryBuff(string buffName, string buffCode, AttributeType type, AttributeModifier modifier, float duration, Sprite manualIcon = null, BuffCategory category = BuffCategory.Default, int maxStacks = 1)
        {
            if (attributeSystem == null) return;

            ActiveBuff existingBuff = activeBuffs.Find(b => b.BuffCode == buffCode && b.TargetAttribute == type);
            if (existingBuff != null)
            {
                existingBuff.RemainingDuration = duration;
                existingBuff.TotalDuration = duration;

                if (existingBuff.Stacks < maxStacks)
                {
                    existingBuff.Modifiers.Add(modifier);
                    attributeSystem.AddModifier(type, modifier);
                }
                return;
            }

            Sprite finalIcon = manualIcon;
            if (finalIcon == null)
            {
                string categoryIconName = "";
                switch (category)
                {
                    case BuffCategory.Aura: categoryIconName = "Icon_Aura_Buff"; break;
                    case BuffCategory.Attribute: categoryIconName = "Icon_Attribute_Buff"; break;
                    case BuffCategory.Skill: categoryIconName = "Icon_Skill_Buff"; break;
                    case BuffCategory.Effect: categoryIconName = "Icon_Effect_Buff"; break;
                }

                if (!string.IsNullOrEmpty(categoryIconName))
                {
                    finalIcon = BuffIconLoader.Load(categoryIconName);
                }

                if (finalIcon == null)
                {
                    finalIcon = BuffIconLoader.GetFallback();
                }
            }

            ActiveBuff buff = new ActiveBuff
            {
                BuffName = buffName,
                BuffCode = buffCode,
                TargetAttribute = type,
                RemainingDuration = duration,
                TotalDuration = duration,
                Icon = finalIcon,
                Category = category,
                MaxStacks = maxStacks
            };
            buff.Modifiers.Add(modifier);

            activeBuffs.Add(buff);
            attributeSystem.AddModifier(type, modifier);
            OnBuffsChanged?.Invoke();
        }

        public void ApplyTemporaryBuffWithTick(string buffName, string buffCode, AttributeType type, AttributeModifier modifier, float duration, float tickInterval, System.Action onTick, Sprite manualIcon = null, BuffCategory category = BuffCategory.Default, int maxStacks = 1)
        {
            if (attributeSystem == null) return;

            ActiveBuff existingBuff = activeBuffs.Find(b => b.BuffCode == buffCode && b.TargetAttribute == type);
            if (existingBuff != null)
            {
                existingBuff.RemainingDuration = duration;
                existingBuff.TotalDuration = duration;
                existingBuff.OnTick = onTick;
                existingBuff.TickInterval = tickInterval;

                if (existingBuff.Stacks < maxStacks)
                {
                    existingBuff.Modifiers.Add(modifier);
                    attributeSystem.AddModifier(type, modifier);
                }
                return;
            }

            Sprite finalIcon = manualIcon;
            if (finalIcon == null)
            {
                string categoryIconName = "";
                switch (category)
                {
                    case BuffCategory.Aura: categoryIconName = "Icon_Aura_Buff"; break;
                    case BuffCategory.Attribute: categoryIconName = "Icon_Attribute_Buff"; break;
                    case BuffCategory.Skill: categoryIconName = "Icon_Skill_Buff"; break;
                    case BuffCategory.Effect: categoryIconName = "Icon_Effect_Buff"; break;
                }

                if (!string.IsNullOrEmpty(categoryIconName))
                {
                    finalIcon = BuffIconLoader.Load(categoryIconName);
                }

                if (finalIcon == null)
                {
                    finalIcon = BuffIconLoader.GetFallback();
                }
            }

            ActiveBuff buff = new ActiveBuff
            {
                BuffName = buffName,
                BuffCode = buffCode,
                TargetAttribute = type,
                RemainingDuration = duration,
                TotalDuration = duration,
                Icon = finalIcon,
                Category = category,
                MaxStacks = maxStacks,
                TickInterval = tickInterval,
                TickTimer = 0f,
                OnTick = onTick
            };
            buff.Modifiers.Add(modifier);

            activeBuffs.Add(buff);
            attributeSystem.AddModifier(type, modifier);
            OnBuffsChanged?.Invoke();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            bool anyRemoved = false;
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                ActiveBuff buff = activeBuffs[i];
                buff.RemainingDuration -= dt;

                if (buff.TickInterval > 0f && buff.OnTick != null)
                {
                    buff.TickTimer += dt;
                    if (buff.TickTimer >= buff.TickInterval)
                    {
                        buff.TickTimer -= buff.TickInterval;
                        buff.OnTick.Invoke();
                    }
                }

                if (buff.RemainingDuration <= 0f)
                {
                    if (attributeSystem != null)
                    {
                        foreach (var mod in buff.Modifiers)
                        {
                            attributeSystem.RemoveModifier(buff.TargetAttribute, mod);
                        }
                    }
                    activeBuffs.RemoveAt(i);
                    anyRemoved = true;
                }
            }
            if (anyRemoved)
            {
                OnBuffsChanged?.Invoke();
            }
        }
    }
}
