using UnityEngine;
using System.Collections.Generic;
using CoreSystem.Controllers;
using System.Linq;

namespace CoreSystem.Entity
{
    public enum RoutineActionType
    {
        Idle,           // AI stays still, clears command
        Roam,           // AI goes back to AgentCore native roaming
        MoveToPoint,    // AI forced to move to a specific Transform point and stays there
        FollowTarget    // AI forced to follow a specific Transform
    }
}
