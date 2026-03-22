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

        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (payload.Caster == null) return;

            switch (Type)
            {
                case MovementType.TeleportToTarget:
                    if (target != null)
                    {
                        payload.Caster.transform.position = target.transform.position;
                        Debug.Log($"Teleported to target {target}");
                    }
                    else
                    {
                        // Fallback: Teleport forward
                        payload.Caster.transform.position += payload.Caster.transform.up * Distance;
                        Debug.Log("Teleported forward (no target found)");
                    }
                    break;
                    
                case MovementType.DashForward:
                    Vector3 forwardDir = payload.Caster.transform.up; // Assuming 2D Top-down default rotation
                    payload.Caster.transform.position += forwardDir * Distance;
                    Debug.Log($"Dashed forward {Distance} units");
                    break;

                case MovementType.DashToTarget:
                    if (target != null)
                    {
                        Vector3 dirToTarget = (target.transform.position - payload.Caster.transform.position).normalized;
                        payload.Caster.transform.position += dirToTarget * Distance;
                        Debug.Log($"Dashed to target {target}");
                    }
                    else
                    {
                        // Fallback: Dash forward
                        Vector3 dirForw = payload.Caster.transform.up;
                        payload.Caster.transform.position += dirForw * Distance;
                        Debug.Log("Dashed forward (no target found)");
                    }
                    break;

                case MovementType.PushTargetAway:
                    if (target != null)
                    {
                        Vector3 pushDir = (target.transform.position - payload.Caster.transform.position).normalized;
                        target.transform.position += pushDir * Distance;
                        Debug.Log($"Pushed target {target} away");
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
                        target.transform.position += pullDir * Distance;
                        Debug.Log($"Pulled target {target} in");
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
