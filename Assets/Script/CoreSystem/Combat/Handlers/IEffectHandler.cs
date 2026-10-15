using System;
using UnityEngine.Scripting;

namespace CoreSystem.Combat
{
    public interface IEffectHandler
    {
        TargetRule TargetRule { get; }
        Outcome Resolve(EffectSpec spec, ResolveContext context);
        void Apply(Outcome outcome, ActionResult result);
    }
}
