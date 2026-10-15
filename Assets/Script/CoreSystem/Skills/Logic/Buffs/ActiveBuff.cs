using System.Collections.Generic;
using CoreSystem.Attributes;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
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
}
