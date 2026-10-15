using UnityEngine;
using System.Collections.Generic;
using CoreSystem.Controllers;
using System.Linq;

namespace CoreSystem.Entity
{
    [RequireComponent(typeof(NonPlayerCharacter))]
    public class NPCRoutine : MonoBehaviour
    {
        #region [Variables/Fields]
        [Header("Routine Schedule")]
        public List<NPCRoutineTask> DailySchedule = new List<NPCRoutineTask>();

        private NonPlayerCharacter _npc;
        private NPCRoutineTask _currentTask;
        private bool _isInitialized = false;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _npc = GetComponent<NonPlayerCharacter>();
        }

        private void Start()
        {
            if (GlobalGameTime.Instance != null)
            {
                GlobalGameTime.Instance.OnTimeChanged += HandleTimeChanged;
                _isInitialized = true;

                // Fire initial sync
                SyncRoutineToCurrentTime(GlobalGameTime.Instance.CurrentHour, GlobalGameTime.Instance.CurrentMinute);
            }
            else
            {
                Debug.LogWarning("[NPCRoutine] GlobalGameTime is missing in the scene!");
            }
        }

        private void OnDestroy()
        {
            if (_isInitialized && GlobalGameTime.Instance != null)
            {
                GlobalGameTime.Instance.OnTimeChanged -= HandleTimeChanged;
            }
        }
        #endregion

        #region [Private Methods]
        private void HandleTimeChanged(int hour, int minute)
        {
            // Check if any task exactly matches current time
            var matchingTask = DailySchedule.FirstOrDefault(t => t.Hour == hour && t.Minute == minute);

            if (matchingTask != null && matchingTask != _currentTask)
            {
                ExecuteTask(matchingTask);
            }
        }

        private void SyncRoutineToCurrentTime(int currentHour, int currentMinute)
        {
            if (DailySchedule.Count == 0) return;

            // Find the most recently passed task
            int currentTotalMinutes = (currentHour * 60) + currentMinute;
            
            NPCRoutineTask latestTask = null;
            int smallestDiff = int.MaxValue;

            foreach (var task in DailySchedule)
            {
                int taskTotalMinutes = (task.Hour * 60) + task.Minute;
                int diff = currentTotalMinutes - taskTotalMinutes;

                // Handle tasks from previous day (wrap around)
                if (diff < 0)
                {
                    diff += 24 * 60;
                }

                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    latestTask = task;
                }
            }

            if (latestTask != null)
            {
                ExecuteTask(latestTask);
            }
        }

        private void ExecuteTask(NPCRoutineTask task)
        {
            _currentTask = task;
            
            switch (task.ActionType)
            {
                case RoutineActionType.Idle:
                    _npc.ClearCommands();
                    // Disable roam on AgentCore so they just stand in place
                    if (_npc.Agent != null) _npc.Agent.CanRoam = false;
                    break;
                case RoutineActionType.Roam:
                    _npc.ClearCommands();
                    // Ensure roam is enabled
                    if (_npc.Agent != null) _npc.Agent.CanRoam = true;
                    break;
                case RoutineActionType.MoveToPoint:
                    if (_npc.Agent != null) _npc.Agent.CanRoam = false; // Override roam
                    if (task.TargetPoint != null)
                    {
                        _npc.CommandMoveTo(task.TargetPoint.position);
                    }
                    break;
                case RoutineActionType.FollowTarget:
                    if (_npc.Agent != null) _npc.Agent.CanRoam = false; // Override roam
                    if (task.TargetPoint != null)
                    {
                        _npc.CommandFollow(task.TargetPoint);
                    }
                    break;
            }
        }
        #endregion
    }
}
