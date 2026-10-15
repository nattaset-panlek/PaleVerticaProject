using UnityEngine;
using System.Collections.Generic;
using CoreSystem.Controllers;
using System.Linq;

namespace CoreSystem.Entity
{
    [System.Serializable]
    public class NPCRoutineTask
    {
        public string TaskName;
        [Range(0, 23)] public int Hour;
        [Range(0, 59)] public int Minute;
        public RoutineActionType ActionType;
        public Transform TargetPoint;   // Used for MoveToPoint and FollowTarget
    }
}
