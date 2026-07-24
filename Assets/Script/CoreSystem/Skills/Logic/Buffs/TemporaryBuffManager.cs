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
        public string SourceName;
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

        private void Awake()
        {
            var entity = GetComponent<Entity.IEntity>();
            if (entity != null)
            {
                attributeSystem = entity.attributeSystem;
            }
        }
        public void ApplyTemporaryBuff(string sourceName, AttributeType type, AttributeModifier modifier, float duration, Sprite manualIcon = null, BuffCategory category = BuffCategory.Default, int maxStacks = 1)
        {
            if (attributeSystem == null) return;

            ActiveBuff existingBuff = activeBuffs.Find(b => b.SourceName == sourceName && b.TargetAttribute == type);
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
                SourceName = sourceName,
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
        }

        public void ApplyTemporaryBuffWithTick(string sourceName, AttributeType type, AttributeModifier modifier, float duration, float tickInterval, System.Action onTick, Sprite manualIcon = null, BuffCategory category = BuffCategory.Default, int maxStacks = 1)
        {
            if (attributeSystem == null) return;

            ActiveBuff existingBuff = activeBuffs.Find(b => b.SourceName == sourceName && b.TargetAttribute == type);
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
                SourceName = sourceName,
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
        }

        private void Update()
        {
            float dt = Time.deltaTime;
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
                }
            }
        }
    }
}
