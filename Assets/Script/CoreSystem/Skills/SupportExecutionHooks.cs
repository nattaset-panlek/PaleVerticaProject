using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Skills
{
    public interface IPayloadSplitter
    {
        bool CanSplit(ActiveSkill skill);
        IReadOnlyList<SkillPayload> Split(SkillPayload payload, Vector3 casterPosition);
    }

    public interface ICastRepeater
    {
        float RepeatDelay { get; }
        SkillPayload CreateRepeatPayload(SkillPayload original);
    }
}
