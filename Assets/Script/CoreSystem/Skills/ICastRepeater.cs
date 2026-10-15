using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Skills
{
    public interface ICastRepeater
    {
        float RepeatDelay { get; }
        SkillPayload CreateRepeatPayload(SkillPayload original);
    }
}
