using UnityEngine;

namespace CoreSystem.Skills
{
    public abstract class SupportModifier : ScriptableObject, ISupportModifier
    {
        public int ID;
        public abstract void ModifyPayload(ref SkillPayload payload);
    }
}
