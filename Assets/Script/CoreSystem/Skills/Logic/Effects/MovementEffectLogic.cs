using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic.Effects
{
    public enum MovementType
    {
        TeleportToTarget,
        DashForward,
        DashToTarget,
        PushTargetAway,
        PullTargetIn
    }

    [CreateAssetMenu(fileName = "NewMovementEffect", menuName = "Skill System/Effects/Movement")]
    public class MovementEffectLogic : EffectLogic
    {
        public MovementType Type;
        
        [Tooltip("How far to dash or push. Ignored for TeleportToTarget.")]
        public float Distance = 5f;
        
        [Tooltip("If true, this effect only runs once per skill cast (useful for Caster dashes to avoid dashing 5 times if hitting 5 enemies).")]
        public bool ApplyOncePerCast = true;

        [Header("Interpolation")]
        public bool UseLerp = false;
        public float LerpDuration = 0.2f;

        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (payload.Caster == null) return;

            switch (Type)
            {
                case MovementType.TeleportToTarget:
                    if (target != null)
                    {
                        if (UseLerp)
                        {
                            Vector3 dir = (target.transform.position - payload.Caster.transform.position).normalized;
                            float dist = Vector3.Distance(payload.Caster.transform.position, target.transform.position);
                            payload.Caster.ApplyImpulse(dir, dist, LerpDuration);
                        }
                        else
                        {
                            payload.Caster.transform.position = target.transform.position;
                        }
                    }
                    else
                    {
                        // Fallback: Teleport forward
                        if (UseLerp)
                            payload.Caster.ApplyImpulse(payload.Caster.transform.up, Distance, LerpDuration);
                        else
                            payload.Caster.transform.position += payload.Caster.transform.up * Distance;
                    }
                    break;
                    
                case MovementType.DashForward:
                    Vector3 forwardDir = payload.Caster.transform.up; // Assuming 2D Top-down default rotation
                    if (UseLerp)
                        payload.Caster.ApplyImpulse(forwardDir, Distance, LerpDuration);
                    else
                        payload.Caster.transform.position += forwardDir * Distance;
                    break;

                case MovementType.DashToTarget:
                    if (target != null)
                    {
                        Vector3 dirToTarget = (target.transform.position - payload.Caster.transform.position).normalized;
                        if (UseLerp)
                        {
                            float actualDist = Vector3.Distance(payload.Caster.transform.position, target.transform.position);
                            payload.Caster.ApplyImpulse(dirToTarget, Mathf.Min(Distance, actualDist), LerpDuration);
                        }
                        else
                        {
                            payload.Caster.transform.position += dirToTarget * Distance;
                        }
                    }
                    else
                    {
                        // Fallback: Dash forward
                        if (UseLerp)
                            payload.Caster.ApplyImpulse(payload.Caster.transform.up, Distance, LerpDuration);
                        else
                            payload.Caster.transform.position += payload.Caster.transform.up * Distance;
                    }
                    break;

                case MovementType.PushTargetAway:
                    if (target != null)
                    {
                        Vector3 pushDir = (target.transform.position - payload.Caster.transform.position).normalized;
                        if (UseLerp)
                            target.ApplyImpulse(pushDir, Distance, LerpDuration);
                        else
                            target.transform.position += pushDir * Distance;
                    }
                    else
                    {
                        // Removed spammy log
                    }
                    break;

                case MovementType.PullTargetIn:
                    if (target != null)
                    {
                        Vector3 pullDir = (payload.Caster.transform.position - target.transform.position).normalized;
                        if (UseLerp)
                        {
                            float actualDist = Vector3.Distance(payload.Caster.transform.position, target.transform.position);
                            target.ApplyImpulse(pullDir, Mathf.Min(Distance, actualDist), LerpDuration);
                        }
                        else
                        {
                            target.transform.position += pullDir * Distance;
                        }
                    }
                    else
                    {
                        // Removed spammy log
                    }
                    break;
            }
        }
    }
}
