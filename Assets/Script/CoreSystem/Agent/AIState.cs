using CoreSystem.Entity;
using CoreSystem.Enums;
using CoreSystem.Pathfinding;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Controllers
{
    public enum AIState
    {
        Idle,
        Roam,
        Chase,
        Attack,
        Flee,
        Dead,
        CommandMove,
        CommandFollow
    }
}
