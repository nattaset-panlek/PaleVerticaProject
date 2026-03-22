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

            // Check if buff already exists
            ActiveBuff existingBuff = activeBuffs.Find(b => b.SourceName == sourceName && b.TargetAttribute == type);
            if (existingBuff != null)
            {
                // Refresh duration
                existingBuff.RemainingDuration = duration;
                existingBuff.TotalDuration = duration; // Potentially update total if different

                // Handle stacking
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
                // 1. Try category specific icon
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
                    finalIcon = Resources.Load<Sprite>($"Sprite/Buff/{categoryIconName}");
                }

                // 2. If specifically tagged failed or was Default, try Default icon
                if (finalIcon == null)
                {
                    finalIcon = Resources.Load<Sprite>("Sprite/Buff/Icon_Default_Buff");
                }

                if (finalIcon == null)
                {
                    Debug.LogWarning($"[BuffSystem] Missing Icon at Sprite/Buff/Icon_Default_Buff for buff: {sourceName} (Category: {category})");
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

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                activeBuffs[i].RemainingDuration -= dt;
                if (activeBuffs[i].RemainingDuration <= 0f)
                {
                    // Remove all modifiers for this buff
                    if (attributeSystem != null)
                    {
                        foreach (var mod in activeBuffs[i].Modifiers)
                        {
                            attributeSystem.RemoveModifier(activeBuffs[i].TargetAttribute, mod);
                        }
                    }
                    activeBuffs.RemoveAt(i);
                }
            }
        }
    }
}
